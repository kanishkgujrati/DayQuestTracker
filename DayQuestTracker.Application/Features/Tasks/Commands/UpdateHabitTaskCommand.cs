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
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Result<HabitTaskDto>.Failure("Title cannot be empty.");

                task.Title = request.Title.Trim();
            }

            if (request.Description is not null)
                task.Description = request.Description;

            if (request.Difficulty.HasValue)
            {
                if (request.Difficulty.Value < 1 || request.Difficulty.Value > 5)
                    return Result<HabitTaskDto>.Failure("Difficulty must be between 1 and 5.");

                task.Difficulty = request.Difficulty.Value;
            }

            // Handle FrequencyType and TargetPerWeek together — they are coupled
            var newFrequency = request.FrequencyType ?? task.FrequencyType;

            if (request.FrequencyType.HasValue)
                task.FrequencyType = request.FrequencyType.Value;

            if (newFrequency == FrequencyType.Custom)
            {
                var newTarget = request.TargetPerWeek ?? task.TargetPerWeek;
                if (newTarget is null)
                    return Result<HabitTaskDto>.Failure("TargetPerWeek is required for Custom frequency.");
                task.TargetPerWeek = newTarget;
            }
            else
            {
                // Non-custom frequencies must never have TargetPerWeek
                if (request.TargetPerWeek.HasValue)
                    return Result<HabitTaskDto>.Failure("TargetPerWeek can only be set on Custom frequency tasks.");
                task.TargetPerWeek = null;
            }

            // Only replace schedules if ScheduledDays was explicitly sent
            if (request.ScheduledDays is not null)
            {
                if (request.ScheduledDays.Any(d => d < 0 || d > 6))
                    return Result<HabitTaskDto>.Failure("Scheduled days must be between 0 (Monday) and 6 (Sunday).");

                var existingSchedules = task.TaskSchedules.ToList();
                foreach (var schedule in existingSchedules)
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

            var categoryName = task.Category?.Name ?? string.Empty;

            if (request.CategoryId.HasValue)
            {
                var newCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value &&
                                              c.UserId == request.UserId, cancellationToken);
                if (newCategory is null)
                    return Result<HabitTaskDto>.Failure("Category not found.");

                task.CategoryId = newCategory.Id;
                categoryName = newCategory.Name;
            }

            return Result<HabitTaskDto>.Success(new HabitTaskDto
            {
                Id = task.Id,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name ?? string.Empty,
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
