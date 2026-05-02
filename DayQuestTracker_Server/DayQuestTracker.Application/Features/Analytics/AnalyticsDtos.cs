namespace DayQuestTracker.Application.Features.Analytics
{
    public class TaskConsistencyDto
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public int TotalScheduledDays { get; set; }
        public int CompletedDays { get; set; }
        public int SkippedDays { get; set; }
        public int MissedDays { get; set; }
        public double ConsistencyPercent { get; set; }
    }
    public class DailyScoreTrendDto
    {
        public DateOnly Date { get; set; }
        public int Score { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public int XPEarned { get; set; }
    }
    public class TaskStreakSummaryDto
    {
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateOnly? LastCompletedDate { get; set; }
    }
    public class CategoryPerformanceDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public double AverageConsistency { get; set; }
        public int TotalXPEarned { get; set; }
        public int BestStreak { get; set; }
    }
}
