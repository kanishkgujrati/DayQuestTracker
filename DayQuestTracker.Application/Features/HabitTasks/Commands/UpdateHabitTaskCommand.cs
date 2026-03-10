using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Commands
{
    public record UpdateHabitTaskCommand(Guid Id,Guid UserId,Guid? CategoryId,string? Title,string? Description,int? Difficulty
        ,FrequencyType? FrequencyType,int? TargetPerWeek,List<int>? ScheduledDays) : IRequest<Result<HabitTaskDto>>;

    public class UpdateHabitTaskCommandHandler : IRequestHandler<UpdateHabitTaskCommand, Result<HabitTaskDto>>
    {
        private readonly ITrackerDbContext _context;

        public UpdateHabitTaskCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<HabitTaskDto>> Handle(
            UpdateHabitTaskCommand request,
            CancellationToken cancellationToken)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskSchedules)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == request.Id &&
                                          t.UserId == request.UserId,
                                     cancellationToken);

            if (task is null)
                return Result<HabitTaskDto>.Failure("Task not found.");

            var categoryName = task.Category?.Name ?? string.Empty;

            if (request.CategoryId.HasValue)
            {
                var newCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value &&
                                              c.UserId == request.UserId,
                                         cancellationToken);

                if (newCategory is null)
                    return Result<HabitTaskDto>.Failure("Category not found.");

                task.CategoryId = request.CategoryId.Value;
                categoryName = newCategory.Name;
            }

            if (request.Description is not null)
                task.Description = request.Description == string.Empty ? null : request.Description;

            if (request.Title is not null)
                task.Title = request.Title.Trim();

            if (request.Difficulty.HasValue)
            {
                task.Difficulty = request.Difficulty.Value;
            }

            // Handle FrequencyType and TargetPerWeek together — they are coupled
            var newFrequency = request.FrequencyType ?? task.FrequencyType;

            if (request.FrequencyType.HasValue)
            {
                task.FrequencyType = request.FrequencyType.Value;

                if (newFrequency == FrequencyType.Custom)
                {
                    var newTarget = request.TargetPerWeek ?? task.TargetPerWeek;
                    if (newTarget is null)
                        return Result<HabitTaskDto>.Failure(
                            "TargetPerWeek is required for Custom frequency.");
                    task.TargetPerWeek = newTarget;
                }
                else
                {
                    task.TargetPerWeek = null;
                }
            }
            else if (request.TargetPerWeek.HasValue)
            {
                if (task.FrequencyType != FrequencyType.Custom)
                    return Result<HabitTaskDto>.Failure(
                        "TargetPerWeek can only be set on Custom frequency tasks.");

                task.TargetPerWeek = request.TargetPerWeek.Value;
            }

            // Only replace schedules if ScheduledDays was explicitly sent
            if (request.ScheduledDays is not null)
            {
                foreach (var schedule in task.TaskSchedules.ToList())
                    _context.TaskSchedules.Remove(schedule);

                if (newFrequency != FrequencyType.Daily)
                {
                    foreach (var day in request.ScheduledDays.Distinct())
                    {
                        _context.TaskSchedules.Add(new HabitTaskSchedule
                        {
                            HabitTaskId = task.Id,
                            DayOfWeek = day
                        });
                    }
                }
            }
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var freshSchedules = await _context.TaskSchedules.Where(s => s.HabitTaskId == task.Id).Select(s => s.DayOfWeek).ToListAsync(cancellationToken);

            return Result<HabitTaskDto>.Success(new HabitTaskDto
            {
                Id = task.Id,
                CategoryId = task.CategoryId,
                CategoryName = categoryName,
                Title = task.Title,
                Description = task.Description,
                Difficulty = task.Difficulty,
                FrequencyType = task.FrequencyType,
                TargetPerWeek = task.TargetPerWeek,
                ScheduledDays = freshSchedules,
                XPValue = task.XPValue,
                CreatedAt = task.CreatedAt
            });
        }
    }
}
