using DayQuestTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int TotalXP { get; set; } = 0;
        public string Timezone { get; set; } = "UTC";
        public bool IsActive { get; set; } = true;

        // Computed — never stored in DB
        public int Level => (TotalXP / 500) + 1;

        // Navigation properties
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<HabitTask> Tasks { get; set; } = new List<HabitTask>();
        public ICollection<HabitTaskCompletion> TaskCompletions { get; set; } = new List<HabitTaskCompletion>();
        public ICollection<DailyScore> DailyScores { get; set; } = new List<DailyScore>();
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
        public ICollection<XPEvent> XPEvents { get; set; } = new List<XPEvent>();
    }
}
