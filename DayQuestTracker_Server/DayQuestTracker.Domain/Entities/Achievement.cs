using DayQuestTracker.Domain.Common;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Domain.Entities
{
    public class Achievement : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AchievementType AchievementType { get; set; }
        public int ThresholdValue { get; set; }
        public Guid? CategoryId { get; set; }
        public int XPReward { get; set; } = 0;
        public string? Icon { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
