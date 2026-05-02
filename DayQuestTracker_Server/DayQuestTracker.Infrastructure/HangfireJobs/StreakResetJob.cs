using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DayQuestTracker.Infrastructure.HangfireJobs
{
    public class StreakResetJob
    {
        private readonly ITrackerDbContext _context;
        private readonly ILogger<StreakResetJob> _logger;

        public StreakResetJob(ITrackerDbContext context,ILogger<StreakResetJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            var dayOfWeek = (int)yesterday.DayOfWeek == 0
                ? 6
                : (int)yesterday.DayOfWeek - 1;

            _logger.LogInformation("StreakResetJob running for {Date}", yesterday);

            // Get all active streaks where LastCompletedDate is not yesterday
            // and the task was scheduled yesterday
            var streaksToReset = await _context.UserTaskStreaks
                .Include(s => s.Task)
                    .ThenInclude(t => t.TaskSchedules)
                .Where(s => s.CurrentStreak > 0 &&
                            s.Task.DeletedAt == null &&
                            (s.LastCompletedDate == null ||
                             s.LastCompletedDate < yesterday))
                .ToListAsync();

            var resetCount = 0;

            foreach (var streak in streaksToReset)
            {
                // Check if task was scheduled for yesterday
                var wasScheduledYesterday =
                    streak.Task.FrequencyType == FrequencyType.Daily ||
                    streak.Task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek);

                if (!wasScheduledYesterday) continue;

                // Check if there was a completion logged for yesterday
                var hadCompletion = await _context.TaskCompletions
                    .AnyAsync(tc => tc.HabitTaskId == streak.TaskId &&
                                    tc.UserId == streak.UserId &&
                                    tc.CompletionDate == yesterday &&
                                    tc.Status == CompletionStatus.Completed);

                if (!hadCompletion)
                {
                    streak.CurrentStreak = 0;
                    streak.UpdatedAt = DateTime.UtcNow;
                    resetCount++;
                }
            }

            if (resetCount > 0)
                await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation(
                "StreakResetJob completed. Reset {Count} streaks.", resetCount);
        }
    }
}
