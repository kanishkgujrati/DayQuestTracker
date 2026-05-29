using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Domain.Enums;
using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Common.Services
{
    public class DailyScoreService
    {
        private readonly ITrackerDbContext _context;

        public DailyScoreService(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task UpsertAsync(Guid userId, DateOnly date, CancellationToken cancellationToken)
        {
            var dayOfWeek = (int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1;

            var totalTasks = await _context.Tasks
                .Where(t => t.UserId == userId &&
                            DateOnly.FromDateTime(t.CreatedAt) <= date &&
                            (t.DeletedAt == null ||
                             DateOnly.FromDateTime(t.DeletedAt.Value) > date))
                .Where(t => t.FrequencyType == FrequencyType.Daily ||
                            t.FrequencyType == FrequencyType.OnceAWeek ||
                            t.FrequencyType == FrequencyType.OnceAMonth ||
                            t.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                .CountAsync(cancellationToken);

            var completionsForDay = await _context.TaskCompletions
                .Where(tc => tc.UserId == userId &&
                             tc.CompletionDate == date)
                .ToListAsync(cancellationToken);

            var completedCount = completionsForDay
                .Count(c => c.Status == CompletionStatus.Completed);

            var xpEarned = await _context.XPEvents
                .Where(x => x.UserId == userId &&
                            x.TaskCompletionId != null &&
                            _context.TaskCompletions.Any(tc =>
                                tc.Id == x.TaskCompletionId &&
                                tc.CompletionDate == date))
                .SumAsync(x => x.XPAmount, cancellationToken);

            xpEarned = Math.Max(0, xpEarned);

            var score = totalTasks > 0
                ? (int)Math.Round((double)completedCount / totalTasks * 100)
                : 0;

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
