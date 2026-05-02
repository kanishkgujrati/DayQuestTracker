using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Features.Completions
{
    public class TaskCompletionDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateOnly CompletionDate { get; set; }
        public CompletionStatus Status { get; set; }
        public string? Notes { get; set; }
        public int XPAwarded { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DailyCompletionSummaryDto
    {
        public DateOnly Date { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int SkippedTasks { get; set; }
        public int Score { get; set; }
        public int XPEarned { get; set; }
        public List<TaskCompletionDto> Completions { get; set; } = new();
    }
}
