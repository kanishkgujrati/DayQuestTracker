using DayQuestTracker.Domain.Common;

namespace DayQuestTracker.Domain.Entities
{
    public class UserAchievement : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid AchievementId { get; set; }
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Achievement Achievement { get; set; } = null!;
    }
}
