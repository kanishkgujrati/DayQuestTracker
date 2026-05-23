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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var yesterday = today.AddDays(-1);

            _logger.LogInformation("StreakResetJob running for {Date}", yesterday);

            // Get all active streaks with CurrentStreak > 0
            var streaksToCheck = await _context.UserTaskStreaks
                .Include(s => s.Task)
                    .ThenInclude(t => t.TaskSchedules)
                .Where(s => s.CurrentStreak > 0 &&
                            s.Task.DeletedAt == null)
                .ToListAsync();

            var resetCount = 0;

            foreach (var streak in streaksToCheck)
            {
                var task = streak.Task;

                if (DateOnly.FromDateTime(task.CreatedAt) > yesterday)
                    continue;

                // Check if task was scheduled for yesterday
                var dayOfWeek = (int)yesterday.DayOfWeek == 0 ? 6 : (int)yesterday.DayOfWeek - 1;

                var wasScheduledYesterday =
                    task.FrequencyType == Domain.Enums.FrequencyType.Daily ||
                    task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek);

                if (!wasScheduledYesterday) continue;

                // Check if completed yesterday
                var hadCompletion = await _context.TaskCompletions
                    .AnyAsync(tc => tc.HabitTaskId == streak.TaskId &&
                                    tc.UserId == streak.UserId &&
                                    tc.CompletionDate == yesterday &&
                                    tc.Status == Domain.Enums.CompletionStatus.Completed);

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
