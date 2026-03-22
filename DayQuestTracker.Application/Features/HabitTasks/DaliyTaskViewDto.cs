using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Features.HabitTasks
{
    public class DailyTaskViewDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int XPValue { get; set; }
        public FrequencyType FrequencyType { get; set; }

        // Completion details — null if not yet logged
        public Guid? CompletionId { get; set; }
        public CompletionStatus? Status { get; set; }
        public string? Notes { get; set; }

        // Streak info for motivation
        public int CurrentStreak { get; set; }
    }
}
