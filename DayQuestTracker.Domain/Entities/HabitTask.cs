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
        public FrequencyType FrequencyType { get; set; }
        public int? TargetPerWeek { get; set; }

        // Computed XP value for this task
        public int XPValue => FrequencyType switch
        {
            FrequencyType.Daily => Difficulty * 10,
            FrequencyType.Weekly => (int)(Difficulty * 10 * 1.5),
            FrequencyType.Custom => (int)(Difficulty * 10 * 1.2),
            _ => Difficulty * 10
        };

        // Navigation properties
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<HabitTaskSchedule> TaskSchedules { get; set; } = new List<HabitTaskSchedule>();
        public ICollection<HabitTaskCompletion> TaskCompletions { get; set; } = new List<HabitTaskCompletion>();
        public UserTaskStreak? Streak { get; set; }
    }
}
