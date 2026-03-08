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

            if (request.CategoryId.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value &&
                                              c.UserId == request.UserId,
                                         cancellationToken);

                if (category is null)
                    return Result<HabitTaskDto>.Failure("Category not found.");

                task.CategoryId = request.CategoryId.Value;
            }

            if (request.Title is not null)
                task.Title = request.Title.Trim();

            if (request.Description is not null)
                task.Description = request.Description;

            if (request.Difficulty.HasValue)
            {
                if (request.Difficulty.Value < 1 || request.Difficulty.Value > 5)
                    return Result<HabitTaskDto>.Failure("Difficulty must be between 1 and 5.");

                task.Difficulty = request.Difficulty.Value;
            }

            // FrequencyType change requires re-evaluating schedules
            if (request.FrequencyType.HasValue)
            {
                if (request.FrequencyType.Value == FrequencyType.Custom &&
                    request.TargetPerWeek is null && task.TargetPerWeek is null)
                    return Result<HabitTaskDto>.Failure("TargetPerWeek is required for Custom frequency.");

                task.FrequencyType = request.FrequencyType.Value;
                task.TargetPerWeek = request.FrequencyType.Value == FrequencyType.Custom
                    ? (request.TargetPerWeek ?? task.TargetPerWeek)
                    : null;
            }

            if (request.TargetPerWeek.HasValue)
                task.TargetPerWeek = request.TargetPerWeek.Value;

            // Only replace schedules if ScheduledDays was explicitly sent
            if (request.ScheduledDays is not null)
            {
                var existingSchedules = task.TaskSchedules.ToList();
                foreach (var schedule in existingSchedules)
                    _context.TaskSchedules.Remove(schedule);

                var currentFrequency = request.FrequencyType ?? task.FrequencyType;
                if (currentFrequency != FrequencyType.Daily)
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

            var updatedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == task.CategoryId, cancellationToken);

            return Result<HabitTaskDto>.Success(new HabitTaskDto
            {
                Id = task.Id,
                CategoryId = task.CategoryId,
                CategoryName = updatedCategory?.Name ?? string.Empty,
                Title = task.Title,
                Description = task.Description,
                Difficulty = task.Difficulty,
                FrequencyType = task.FrequencyType,
                TargetPerWeek = task.TargetPerWeek,
                ScheduledDays = task.TaskSchedules.Select(s => s.DayOfWeek).ToList(),
                XPValue = task.XPValue,
                CreatedAt = task.CreatedAt
            });
        }
    }
}
