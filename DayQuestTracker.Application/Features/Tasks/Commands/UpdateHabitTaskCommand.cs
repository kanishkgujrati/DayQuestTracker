using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Commands
{
    public record UpdateHabitTaskCommand(Guid Id,Guid UserId,Guid CategoryId,string Title,string? Description,int Difficulty
        ,FrequencyType FrequencyType,int? TargetPerWeek,List<int>? ScheduledDays) : IRequest<Result<HabitTaskDto>>;

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

            // Validate category belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId &&
                                          c.UserId == request.UserId,
                                     cancellationToken);

            if (category is null)
                return Result<HabitTaskDto>.Failure("Category not found.");

            if (request.Difficulty < 1 || request.Difficulty > 5)
                return Result<HabitTaskDto>.Failure("Difficulty must be between 1 and 5.");

            if (request.FrequencyType == FrequencyType.Custom && request.TargetPerWeek is null)
                return Result<HabitTaskDto>.Failure("TargetPerWeek is required for Custom frequency.");

            // Update task fields
            task.CategoryId = request.CategoryId;
            task.Title = request.Title.Trim();
            task.Description = request.Description;
            task.Difficulty = request.Difficulty;
            task.FrequencyType = request.FrequencyType;
            task.TargetPerWeek = request.FrequencyType == FrequencyType.Custom
                ? request.TargetPerWeek : null;
            task.UpdatedAt = DateTime.UtcNow;

            // Replace schedules — delete old, insert new
            // Why replace instead of update: simpler logic, no diff calculation needed
            var existingSchedules = task.TaskSchedules.ToList();
            foreach (var schedule in existingSchedules)
                _context.TaskSchedules.Remove(schedule);

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
            }

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
