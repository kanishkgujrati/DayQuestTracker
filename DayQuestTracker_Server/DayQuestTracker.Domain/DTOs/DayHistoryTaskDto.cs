using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Domain.DTOs
{
    public class DayHistoryTaskDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int XPValue { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public DayTaskStatus Status { get; set; }
        public string? Notes { get; set; }
        public int XPAwarded { get; set; }
    }
}
