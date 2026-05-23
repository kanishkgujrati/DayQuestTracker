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

            // Start from task creation date, not from the beginning of time
            var startDate = DateOnly.FromDateTime(CreatedAt);
            var current = startDate;

            while (current <= endDate)
            {
                if (FrequencyType == Enums.FrequencyType.Daily)
                {
                    scheduledDates.Add(current);
                }
                else
                {
                    var dayOfWeek = (int)current.DayOfWeek == 0 ? 6 : (int)current.DayOfWeek - 1;

                    if (TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek))
                        scheduledDates.Add(current);
                }

                current = current.AddDays(1);
            }

            return scheduledDates;
        }
    }
}
