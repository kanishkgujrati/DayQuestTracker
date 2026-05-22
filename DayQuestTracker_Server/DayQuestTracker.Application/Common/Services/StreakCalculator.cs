using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Common.Services
{ 
    public static class StreakCalculator
    {
        public static void Recalculate(UserTaskStreak streak, HabitTask task, List<DateOnly> allCompletedDates, CompletionStatus? justLoggedStatus = null)
        {
            // If just logged a Skip — streak breaks immediately
            if (justLoggedStatus == CompletionStatus.Skipped)
            {
                streak.CurrentStreak = 0;
                streak.UpdatedAt = DateTime.UtcNow;
                return;
            }

            if (!allCompletedDates.Any())
            {
                streak.CurrentStreak = 0;
                streak.LastCompletedDate = null;
                streak.UpdatedAt = DateTime.UtcNow;
                return;
            }

            // LastCompletedDate is always the most recent completed date
            var mostRecentCompleted = allCompletedDates.Max();
            streak.LastCompletedDate = mostRecentCompleted;

            // Get all scheduled dates up to today ordered descending
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var scheduledDates = task.GetScheduledDates(today)
                .OrderByDescending(d => d)
                .ToList();

            // Count consecutive scheduled dates that have a completion
            var completedSet = new HashSet<DateOnly>(allCompletedDates);
            var currentStreak = 0;

            foreach (var scheduledDate in scheduledDates)
            {
                // Only count scheduled dates up to the most recent completion
                // Why: future scheduled dates that haven't happened yet
                // should not break the streak
                if (scheduledDate > mostRecentCompleted)
                    continue;

                if (completedSet.Contains(scheduledDate))
                    currentStreak++;
                else
                    break; // Gap found — stop counting
            }

            streak.CurrentStreak = currentStreak;

            // LongestStreak never decreases
            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;

            streak.UpdatedAt = DateTime.UtcNow;
        }
    }
}
