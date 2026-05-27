namespace DayQuestTracker.Domain.DTOs
{
    public class DayHistoryDto
    {
        public DateOnly Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedCount { get; set; }
        public int SkippedCount { get; set; }
        public int MissedCount { get; set; }
        public int XPEarned { get; set; }
        public List<DayHistoryTaskDto> Tasks { get; set; } = new();
    }
}
