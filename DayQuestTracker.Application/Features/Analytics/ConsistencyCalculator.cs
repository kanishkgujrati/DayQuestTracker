using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Features.Analytics
{
    public static class ConsistencyCalculator
    {
        public static TaskConsistencyDto Calculate(HabitTask task,DateOnly startDate,DateOnly endDate,List<HabitTaskCompletion> completionsForTask)
        {
            var scheduledDays = GetScheduledDays(task, startDate, endDate);
            var totalScheduled = scheduledDays.Count;

            var completedDays = completionsForTask
                .Count(c => c.Status == CompletionStatus.Completed &&
                            scheduledDays.Contains(c.CompletionDate));

            var skippedDays = completionsForTask
                .Count(c => c.Status == CompletionStatus.Skipped &&
                            scheduledDays.Contains(c.CompletionDate));

            var loggedDays = completionsForTask
                .Select(c => c.CompletionDate)
                .Where(d => scheduledDays.Contains(d))
                .Distinct()
                .Count();

            var missedDays = totalScheduled - loggedDays;

            var consistencyPercent = totalScheduled > 0
                ? Math.Round((double)completedDays / totalScheduled * 100, 1)
                : 0;

            return new TaskConsistencyDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                CategoryName = task.Category?.Name ?? string.Empty,
                CategoryColor = task.Category?.Color ?? string.Empty,
                TotalScheduledDays = totalScheduled,
                CompletedDays = completedDays,
                SkippedDays = skippedDays,
                MissedDays = missedDays,
                ConsistencyPercent = consistencyPercent
            };
        }

        // Returns all dates in range when this task was scheduled to run
        private static List<DateOnly> GetScheduledDays(HabitTask task,DateOnly startDate,DateOnly endDate)
        {
            var scheduledDays = new List<DateOnly>();
            var current = startDate;

            while (current <= endDate)
            {
                if (task.FrequencyType == FrequencyType.Daily)
                {
                    scheduledDays.Add(current);
                }
                else
                {
                    // Weekly/Custom — check if this day of week is scheduled
                    // Convert DayOfWeek to 0=Mon, 6=Sun
                    var dayOfWeek = (int)current.DayOfWeek == 0
                        ? 6
                        : (int)current.DayOfWeek - 1;

                    if (task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                        scheduledDays.Add(current);
                }

                current = current.AddDays(1);
            }

            return scheduledDays;
        }
    }
}
