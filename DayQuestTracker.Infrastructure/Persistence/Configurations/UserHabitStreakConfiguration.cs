using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Infrastructure.Persistence.Configurations
{
    public class UserHabitStreakConfiguration : IEntityTypeConfiguration<UserTaskStreak>
    {
        public void Configure(EntityTypeBuilder<UserTaskStreak> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => new { s.TaskId, s.UserId }).IsUnique();
            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("IX_UserTaskStreaks_User");

            builder.HasOne(s => s.Task)
                .WithOne(t => t.Streak)
                .HasForeignKey<UserTaskStreak>(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
