using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Completions.Commands
{
    public record UndoCompletionCommand(
    Guid CompletionId,
    Guid UserId) : IRequest<Result<bool>>;

    public class UndoCompletionCommandHandler
        : IRequestHandler<UndoCompletionCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;

        public UndoCompletionCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(
            UndoCompletionCommand request,
            CancellationToken cancellationToken)
        {
            // Fetch completion, validate ownership
            var completion = await _context.TaskCompletions
                .Include(tc => tc.HabitTask)
                .FirstOrDefaultAsync(tc => tc.Id == request.CompletionId &&
                                           tc.UserId == request.UserId,
                                     cancellationToken);

            if (completion is null)
                return Result<bool>.Failure("Completion record not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (completion.CompletionDate < today.AddDays(-7))
                return Result<bool>.Failure("Cannot undo completions older than 7 days.");

            var completionDate = completion.CompletionDate;
            var wasCompleted = completion.Status == CompletionStatus.Completed;

            // Deduct XP if was Completed
            if (wasCompleted)
            {
                var xpToDeduct = completion.HabitTask?.XPValue ?? 0;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user is not null)
                {
                    user.TotalXP = Math.Max(0, user.TotalXP - xpToDeduct);
                }

                // Log negative XP event for audit trail
                _context.XPEvents.Add(new XPEvent
                {
                    UserId = request.UserId,
                    TaskCompletionId = completion.Id,
                    CategoryId = completion.HabitTask?.CategoryId,
                    XPAmount = -xpToDeduct,
                    Reason = XPReason.UndoCompletion
                });
            }

            // Step 4 — Delete completion record
            _context.TaskCompletions.Remove(completion);

            // Step 5 — Recalculate streak from scratch
            await RecalculateStreakAsync(
                completion.HabitTaskId,
                request.UserId,
                cancellationToken);

            // Step 6 — Recalculate DailyScore for that date
            await RecalculateDailyScoreAsync(
                request.UserId,
                completionDate,
                completion.Id, // exclude this completion since we are removing it
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        private async Task RecalculateStreakAsync(
            Guid taskId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var streak = await _context.UserTaskStreaks
                .FirstOrDefaultAsync(s => s.TaskId == taskId &&
                                          s.UserId == userId,
                                     cancellationToken);

            if (streak is null) return;

            // Get last Completed record for this task excluding the one being deleted
            // Note: EF Core change tracker handles this — the removed entity
            // is excluded from queries in the same context
            var lastCompleted = await _context.TaskCompletions
                .Where(tc => tc.HabitTaskId == taskId &&
                             tc.UserId == userId &&
                             tc.Status == CompletionStatus.Completed)
                .OrderByDescending(tc => tc.CompletionDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastCompleted is null)
            {
                // No completions left — reset everything
                streak.CurrentStreak = 0;
                streak.LastCompletedDate = null;
            }
            else
            {
                streak.LastCompletedDate = lastCompleted.CompletionDate;

                // Recalculate current streak by counting consecutive days back from last completed
                var currentStreak = 0;
                var checkDate = lastCompleted.CompletionDate;

                while (true)
                {
                    var hasCompletion = await _context.TaskCompletions
                        .AnyAsync(tc => tc.HabitTaskId == taskId &&
                                        tc.UserId == userId &&
                                        tc.CompletionDate == checkDate &&
                                        tc.Status == CompletionStatus.Completed,
                                  cancellationToken);

                    if (!hasCompletion) break;

                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }

                streak.CurrentStreak = currentStreak;
            }

            streak.UpdatedAt = DateTime.UtcNow;
        }

        private async Task RecalculateDailyScoreAsync(
            Guid userId,
            DateOnly date,
            Guid excludedCompletionId,
            CancellationToken cancellationToken)
        {
            // Get remaining completions for this day excluding the deleted one
            var completionsForDay = await _context.TaskCompletions
                .Where(tc => tc.UserId == userId &&
                             tc.CompletionDate == date &&
                             tc.Id != excludedCompletionId)
                .ToListAsync(cancellationToken);

            var dayOfWeek = (int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1;

            var totalTasks = await _context.Tasks
                .Where(t => t.UserId == userId && t.DeletedAt == null)
                .Where(t => t.FrequencyType == Domain.Enums.FrequencyType.Daily ||
                            t.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                .CountAsync(cancellationToken);

            var completedCount = completionsForDay.Count(c => c.Status == CompletionStatus.Completed);
            var skippedCount = completionsForDay.Count(c => c.Status == CompletionStatus.Skipped);

            var xpEarned = await _context.XPEvents
                .Where(x => x.UserId == userId &&
                            x.XPAmount > 0 && // only positive XP events
                            x.CreatedAt >= date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) &&
                            x.CreatedAt < date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                .SumAsync(x => x.XPAmount, cancellationToken);

            var score = totalTasks > 0
                ? (int)Math.Round((double)completedCount / totalTasks * 100)
                : 0;

            var dailyScore = await _context.DailyScores
                .FirstOrDefaultAsync(ds => ds.UserId == userId &&
                                           ds.Date == date,
                                     cancellationToken);

            if (dailyScore is null) return;

            dailyScore.Score = score;
            dailyScore.CompletedTasks = completedCount;
            dailyScore.TotalTasks = totalTasks;
            dailyScore.XPEarned = xpEarned;
            dailyScore.UpdatedAt = DateTime.UtcNow;
        }
    }
}
