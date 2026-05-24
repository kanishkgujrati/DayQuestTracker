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
                var shouldReset = false;

                if (task.FrequencyType == FrequencyType.OnceAWeek)
                {
                    // Reset only if yesterday was Sunday and no completion this week
                    if (yesterday.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var monday = yesterday.AddDays(-6);
                        var hadCompletion = await _context.TaskCompletions
                            .AnyAsync(tc => tc.HabitTaskId == streak.TaskId &&
                                            tc.UserId == streak.UserId &&
                                            tc.CompletionDate >= monday &&
                                            tc.CompletionDate <= yesterday &&
                                            tc.Status == CompletionStatus.Completed);

                        shouldReset = !hadCompletion;
                    }
                }
                else if (task.FrequencyType == FrequencyType.OnceAMonth)
                {
                    // Reset only if yesterday was last day of month
                    var lastDayOfMonth = new DateOnly(
                        yesterday.Year,
                        yesterday.Month,
                        DateTime.DaysInMonth(yesterday.Year, yesterday.Month));

                    if (yesterday == lastDayOfMonth)
                    {
                        var firstOfMonth = new DateOnly(yesterday.Year, yesterday.Month, 1);
                        var hadCompletion = await _context.TaskCompletions
                            .AnyAsync(tc => tc.HabitTaskId == streak.TaskId &&
                                            tc.UserId == streak.UserId &&
                                            tc.CompletionDate >= firstOfMonth &&
                                            tc.CompletionDate <= yesterday &&
                                            tc.Status == CompletionStatus.Completed);

                        shouldReset = !hadCompletion;
                    }
                }
                else
                {
                    // Daily, Weekly, Custom — existing day-based logic
                    var dayOfWeek = (int)yesterday.DayOfWeek == 0
                        ? 6
                        : (int)yesterday.DayOfWeek - 1;

                    var wasScheduledYesterday =
                        task.FrequencyType == FrequencyType.Daily ||
                        task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek);

                    if (wasScheduledYesterday)
                    {
                        var hadCompletion = await _context.TaskCompletions
                            .AnyAsync(tc => tc.HabitTaskId == streak.TaskId &&
                                            tc.UserId == streak.UserId &&
                                            tc.CompletionDate == yesterday &&
                                            tc.Status == CompletionStatus.Completed);

                        shouldReset = !hadCompletion;
                    }
                }

                if (shouldReset)
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
