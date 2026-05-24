using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Common.Services
{
    public static class StreakCalculator
    {
        public static void Recalculate(UserTaskStreak streak, HabitTask task, List<DateOnly> allCompletedDates, CompletionStatus? justLoggedStatus = null)
        {
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

            var mostRecentCompleted = allCompletedDates.Max();
            streak.LastCompletedDate = mostRecentCompleted;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (task.FrequencyType == FrequencyType.OnceAWeek)
            {
                RecalculateWeeklyStreak(streak, allCompletedDates, today);
                return;
            }

            if (task.FrequencyType == FrequencyType.OnceAMonth)
            {
                RecalculateMonthlyStreak(streak, allCompletedDates, today);
                return;
            }

            // Daily, Weekly, Custom — scheduled occurrence based
            var scheduledDates = task.GetScheduledDates(today).OrderByDescending(d => d).ToList();

            var completedSet = new HashSet<DateOnly>(allCompletedDates);
            var currentStreak = 0;

            foreach (var scheduledDate in scheduledDates)
            {
                if (scheduledDate > mostRecentCompleted)
                    continue;

                if (completedSet.Contains(scheduledDate))
                    currentStreak++;
                else
                    break;
            }

            streak.CurrentStreak = currentStreak;

            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;

            streak.UpdatedAt = DateTime.UtcNow;
        }

        private static void RecalculateWeeklyStreak(UserTaskStreak streak, List<DateOnly> allCompletedDates, DateOnly today)
        {
            // Get the Monday of each week that has a completion
            var completedWeeks = allCompletedDates
                .Select(d => GetMondayOfWeek(d))
                .Distinct()
                .OrderByDescending(w => w)
                .ToList();

            if (!completedWeeks.Any())
            {
                streak.CurrentStreak = 0;
                streak.UpdatedAt = DateTime.UtcNow;
                return;
            }

            var currentStreak = 0;
            var checkWeek = completedWeeks.First();

            foreach (var completedWeek in completedWeeks)
            {
                if (completedWeek == checkWeek)
                {
                    currentStreak++;
                    checkWeek = checkWeek.AddDays(-7);
                }
                else
                {
                    break;
                }
            }

            streak.CurrentStreak = currentStreak;

            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;

            streak.UpdatedAt = DateTime.UtcNow;
        }

        private static void RecalculateMonthlyStreak(UserTaskStreak streak,List<DateOnly> allCompletedDates,DateOnly today)
        {
            // Get the first day of each month that has a completion
            var completedMonths = allCompletedDates
                .Select(d => new DateOnly(d.Year, d.Month, 1))
                .Distinct()
                .OrderByDescending(m => m)
                .ToList();

            if (!completedMonths.Any())
            {
                streak.CurrentStreak = 0;
                streak.UpdatedAt = DateTime.UtcNow;
                return;
            }

            var currentStreak = 0;
            var checkMonth = completedMonths.First();

            foreach (var completedMonth in completedMonths)
            {
                if (completedMonth == checkMonth)
                {
                    currentStreak++;
                    checkMonth = checkMonth.AddMonths(-1);
                }
                else
                {
                    break;
                }
            }

            streak.CurrentStreak = currentStreak;

            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;

            streak.UpdatedAt = DateTime.UtcNow;
        }

        private static DateOnly GetMondayOfWeek(DateOnly date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            // DayOfWeek: 0=Sun, 1=Mon...6=Sat
            // We want 0=Mon so Sunday becomes 6
            var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            return date.AddDays(-daysFromMonday);
        }
    }
}