using DayQuestTracker.Domain.Common;
using DayQuestTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Domain.Entities
{
    public class HabitTaskCompletion : BaseEntity
    {
        public Guid HabitTaskId { get; set; }
        public Guid UserId { get; set; }
        public DateOnly CompletionDate { get; set; }
        public CompletionStatus Status { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public HabitTask HabitTask { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
