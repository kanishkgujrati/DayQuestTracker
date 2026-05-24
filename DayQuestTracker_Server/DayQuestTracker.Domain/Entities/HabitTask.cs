using DayQuestTracker.Domain.Common;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Domain.Entities
{
    public class HabitTask : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Difficulty { get; set; } = 1;
        public FrequencyType FrequencyType { get; set; }   // ENUM: Daily / Weekly / Custom
        public int? TargetPerWeek { get; set; }            // INT, nullable (only populated when FrequencyType = Custom)

        // Computed XP value for this task
        public int XPValue => FrequencyType switch
        {
            FrequencyType.Daily => Difficulty * 10 * 2,
            FrequencyType.Weekly => (int)(Difficulty * 10),
            FrequencyType.Custom => (int)(Difficulty * 10 * 1.5),
            FrequencyType.OnceAWeek => (int)(Difficulty * 10 * 2.0),
            FrequencyType.OnceAMonth => (int)(Difficulty * 10 * 3.0),
            _ => Difficulty * 10
        };

        // Navigation properties
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<HabitTaskSchedule> TaskSchedules { get; set; } = new List<HabitTaskSchedule>();
        public ICollection<HabitTaskCompletion> TaskCompletions { get; set; } = new List<HabitTaskCompletion>();
        public UserTaskStreak? Streak { get; set; }

        // Returns all dates this task was scheduled to run up to a given end date
        public List<DateOnly> GetScheduledDates(DateOnly endDate)
        {
            var scheduledDates = new List<DateOnly>();
            var startDate = DateOnly.FromDateTime(CreatedAt);

            if (FrequencyType == Enums.FrequencyType.OnceAWeek)
            {
                // Return last day (Sunday) of each Mon-Sun week
                // from task creation to endDate
                var current = startDate;

                // Move to the Sunday of the first week
                var daysUntilSunday = (7 - (int)current.DayOfWeek) % 7;
                if (daysUntilSunday == 0) daysUntilSunday = 7;
                var firstSunday = current.AddDays(daysUntilSunday);

                var sunday = firstSunday;
                while (sunday <= endDate)
                {
                    scheduledDates.Add(sunday);
                    sunday = sunday.AddDays(7);
                }

                return scheduledDates;
            }

            if (FrequencyType == Enums.FrequencyType.OnceAMonth)
            {
                // Return last day of each calendar month
                // from task creation to endDate
                var currentMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                while (currentMonth <= endDate)
                {
                    var lastDay = new DateOnly(
                        currentMonth.Year,
                        currentMonth.Month,
                        DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));

                    var effectiveLastDay = lastDay <= endDate ? lastDay : endDate;
                    scheduledDates.Add(effectiveLastDay);

                    currentMonth = currentMonth.AddMonths(1);
                }

                return scheduledDates;
            }

            // Daily, Weekly, Custom — existing logic
            var day = startDate;
            while (day <= endDate)
            {
                if (FrequencyType == Enums.FrequencyType.Daily)
                {
                    scheduledDates.Add(day);
                }
                else
                {
                    var dayOfWeek = (int)day.DayOfWeek == 0
                        ? 6
                        : (int)day.DayOfWeek - 1;

                    if (TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                        scheduledDates.Add(day);
                }

                day = day.AddDays(1);
            }

            return scheduledDates;
        }
    }
}
