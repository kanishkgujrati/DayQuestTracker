using DayQuestTracker.Domain.Common;
using DayQuestTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Domain.Entities
{
    public class XPEvent : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? TaskCompletionId { get; set; }
        public Guid? CategoryId { get; set; }
        public int XPAmount { get; set; }
        public XPReason Reason { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public HabitTaskCompletion? TaskCompletion { get; set; }
        public Category? Category { get; set; }
    }
}
