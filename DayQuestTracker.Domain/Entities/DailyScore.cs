using DayQuestTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Domain.Entities
{
    public class DailyScore : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateOnly Date { get; set; }
        public int Score { get; set; } = 0;
        public int CompletedTasks { get; set; } = 0;
        public int TotalTasks { get; set; } = 0;
        public int XPEarned { get; set; } = 0;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
