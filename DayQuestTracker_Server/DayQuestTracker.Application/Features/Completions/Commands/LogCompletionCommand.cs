using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DayQuestTracker.Application.Common.Services;

namespace DayQuestTracker.Application.Features.Completions.Commands
{
    public record LogCompletionCommand(Guid TaskId, Guid UserId, DateOnly CompletionDate, CompletionStatus Status, string? Notes) : IRequest<Result<TaskCompletionDto>>;

    public class LogCompletionCommandHandler : IRequestHandler<LogCompletionCommand, Result<TaskCompletionDto>>
    {
        private readonly ITrackerDbContext _context;

        public LogCompletionCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }
        public async Task<Result<TaskCompletionDto>> Handle(LogCompletionCommand request, CancellationToken cancellationToken)
        {
            // Fetch task WITH schedules — needed for frequency-aware streak calculation
            var task = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId &&
                                          t.UserId == request.UserId,
                                     cancellationToken);

            if (task is null)
                return Result<TaskCompletionDto>.Failure("Task not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (request.CompletionDate > today)
                return Result<TaskCompletionDto>.Failure("Cannot log completions for future dates.");

            if (request.CompletionDate < today.AddDays(-7))
                return Result<TaskCompletionDto>.Failure("Cannot log completions older than 7 days.");

            var existingCompletion = await _context.TaskCompletions
                .FirstOrDefaultAsync(tc => tc.HabitTaskId == request.TaskId &&
                                           tc.UserId == request.UserId &&
                                           tc.CompletionDate == request.CompletionDate,
                                     cancellationToken);

            if (existingCompletion is not null)
                return Result<TaskCompletionDto>.Failure(
                    "A completion record already exists for this task on this date.");

            var completion = new HabitTaskCompletion
            {
                HabitTaskId = request.TaskId,
                UserId = request.UserId,
                CompletionDate = request.CompletionDate,
                Status = request.Status,
                Notes = request.Notes
            };

            _context.TaskCompletions.Add(completion);

            var xpAwarded = 0;

            if (request.Status == CompletionStatus.Completed)
            {
                xpAwarded = task.XPValue;

                _context.XPEvents.Add(new XPEvent
                {
                    UserId = request.UserId,
                    TaskCompletionId = completion.Id,
                    CategoryId = task.CategoryId,
                    XPAmount = xpAwarded,
                    Reason = XPReason.TaskCompletion,
                    CreatedAt = DateTime.UtcNow
                });

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user is not null)
                    user.TotalXP += xpAwarded;
            }

            // First save — commit completion to DB before streak recalculation
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch all completed dates AFTER save
            var allCompletedDates = await _context.TaskCompletions
                .Where(tc => tc.HabitTaskId == request.TaskId &&
                             tc.UserId == request.UserId &&
                             tc.Status == CompletionStatus.Completed)
                .Select(tc => tc.CompletionDate)
                .ToListAsync(cancellationToken);

            // Recalculate streak using frequency-aware logic
            var streak = await _context.UserTaskStreaks
                .FirstOrDefaultAsync(s => s.TaskId == request.TaskId &&
                                          s.UserId == request.UserId,
                                     cancellationToken);

            if (streak is not null)
            {
                StreakCalculator.Recalculate(streak,task,allCompletedDates,request.Status);
            }

            await UpsertDailyScoreAsync(request.UserId,request.CompletionDate,cancellationToken);

            // Second save — persist streak and daily score updates
            await _context.SaveChangesAsync(cancellationToken);

            return Result<TaskCompletionDto>.Success(new TaskCompletionDto
            {
                Id = completion.Id,
                TaskId = completion.HabitTaskId,
                TaskTitle = task.Title,
                CategoryName = task.Category?.Name ?? string.Empty,
                CompletionDate = completion.CompletionDate,
                Status = completion.Status,
                Notes = completion.Notes,
                XPAwarded = xpAwarded,
                CreatedAt = completion.CreatedAt
            });
        }

        private async Task UpsertDailyScoreAsync(Guid userId, DateOnly date, CancellationToken cancellationToken)
        {
            // Get all completions for user on this date
            var completionsForDay = await _context.TaskCompletions
                .Where(tc => tc.UserId == userId && tc.CompletionDate == date)
                .ToListAsync(cancellationToken);

            // Count active tasks scheduled for this day
            var dayOfWeek = (int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1; // Convert to 0=Mon, 6=Sun

            var totalTasks = await _context.Tasks
                .Where(t => t.UserId == userId && t.DeletedAt == null)
                .Where(t => t.FrequencyType == Domain.Enums.FrequencyType.Daily ||
                            t.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                .CountAsync(cancellationToken);

            var completedCount = completionsForDay.Count(c => c.Status == CompletionStatus.Completed);

            var xpEarned = await _context.XPEvents
                .Where(x => x.UserId == userId &&
                            x.TaskCompletionId != null &&
                            _context.TaskCompletions.Any(tc => tc.Id == x.TaskCompletionId && tc.CompletionDate == date))
                .SumAsync(x => x.XPAmount, cancellationToken);
            xpEarned = Math.Max(0, xpEarned);

            var score = totalTasks > 0
                ? (int)Math.Round((double)completedCount / totalTasks * 100)
                : 0;

            // Upsert — update if exists, insert if not
            var dailyScore = await _context.DailyScores
                .FirstOrDefaultAsync(ds => ds.UserId == userId &&
                                           ds.Date == date,
                                     cancellationToken);

            if (dailyScore is null)
            {
                _context.DailyScores.Add(new DailyScore
                {
                    UserId = userId,
                    Date = date,
                    Score = score,
                    CompletedTasks = completedCount,
                    TotalTasks = totalTasks,
                    XPEarned = xpEarned
                });
            }
            else
            {
                dailyScore.Score = score;
                dailyScore.CompletedTasks = completedCount;
                dailyScore.TotalTasks = totalTasks;
                dailyScore.XPEarned = xpEarned;
                dailyScore.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}