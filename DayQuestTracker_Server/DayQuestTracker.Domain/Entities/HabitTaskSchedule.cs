using DayQuestTracker.Domain.Common;

namespace DayQuestTracker.Domain.Entities
{
    public class HabitTaskSchedule : BaseEntity
    {
        public Guid HabitTaskId { get; set; }
        public int DayOfWeek { get; set; } // 0=Mon, 6=Sun

        // Navigation properties
        public HabitTask HabitTask { get; set; } = null!;
    }
}
