using DayQuestTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Domain.Entities
{
    public class UserTaskStreak : BaseEntity
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public DateOnly? LastCompletedDate { get; set; }

        // Navigation properties
        public HabitTask Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
