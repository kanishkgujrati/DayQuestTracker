using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Features.Tasks
{
    public class HabitTaskDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Difficulty { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public int? TargetPerWeek { get; set; }
        public List<int> ScheduledDays { get; set; } = new(); // 0=Mon, 6=Sun
        public int XPValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
