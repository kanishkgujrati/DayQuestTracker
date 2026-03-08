using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Commands
{
    public record CreateHabitTaskCommand(Guid UserId,Guid CategoryId,string Title,string? Description,int Difficulty,
        FrequencyType FrequencyType,int? TargetPerWeek,List<int>? ScheduledDays) : IRequest<Result<HabitTaskDto>>;

    public class CreateHabitTaskCommandHandler : IRequestHandler<CreateHabitTaskCommand, Result<HabitTaskDto>>
    {
        private readonly ITrackerDbContext _context;

        public CreateHabitTaskCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<HabitTaskDto>> Handle(
            CreateHabitTaskCommand request,
            CancellationToken cancellationToken)
        {
            // Validate category belongs to this user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId &&
                                          c.UserId == request.UserId,
                                     cancellationToken);

            if (category is null)
                return Result<HabitTaskDto>.Failure("Category not found.");

            // Business rule: Custom requires TargetPerWeek
            if (request.FrequencyType == FrequencyType.Custom && request.TargetPerWeek is null)
                return Result<HabitTaskDto>.Failure("TargetPerWeek is required for Custom frequency.");

            // Business rule: Daily tasks don't need schedules
            if (request.FrequencyType == FrequencyType.Daily && request.ScheduledDays?.Any() == true)
                return Result<HabitTaskDto>.Failure("Daily tasks do not require scheduled days.");

            // Business rule: Weekly/Custom require scheduled days
            if (request.FrequencyType != FrequencyType.Daily &&
                (request.ScheduledDays is null || !request.ScheduledDays.Any()))
                return Result<HabitTaskDto>.Failure("Weekly and Custom tasks require at least one scheduled day.");

            // Business rule: Difficulty must be 1-5
            if (request.Difficulty < 1 || request.Difficulty > 5)
                return Result<HabitTaskDto>.Failure("Difficulty must be between 1 and 5.");

            var task = new HabitTask
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                Title = request.Title.Trim(),
                Description = request.Description,
                Difficulty = request.Difficulty,
                FrequencyType = request.FrequencyType,
                TargetPerWeek = request.FrequencyType == FrequencyType.Custom
                    ? request.TargetPerWeek
                    : null
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);

            // Create schedules for Weekly/Custom tasks
            if (request.FrequencyType != FrequencyType.Daily && request.ScheduledDays != null)
            {
                foreach (var day in request.ScheduledDays.Distinct())
                {
                    _context.TaskSchedules.Add(new HabitTaskSchedule
                    {
                        HabitTaskId = task.Id,
                        DayOfWeek = day
                    });
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Create streak record for this task
            _context.UserTaskStreaks.Add(new UserTaskStreak
            {
                TaskId = task.Id,
                UserId = request.UserId
            });
            await _context.SaveChangesAsync(cancellationToken);

            return Result<HabitTaskDto>.Success(new HabitTaskDto
            {
                Id = task.Id,
                CategoryId = task.CategoryId,
                CategoryName = category.Name,
                Title = task.Title,
                Description = task.Description,
                Difficulty = task.Difficulty,
                FrequencyType = task.FrequencyType,
                TargetPerWeek = task.TargetPerWeek,
                ScheduledDays = request.ScheduledDays ?? new List<int>(),
                XPValue = task.XPValue,
                CreatedAt = task.CreatedAt
            });
        }
    }
}
