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
    public class HabitTaskCompletionConfiguration : IEntityTypeConfiguration<HabitTaskCompletion>
    {
        public void Configure(EntityTypeBuilder<HabitTaskCompletion> builder)
        {
            builder.HasKey(tc => tc.Id);

            builder.Property(tc => tc.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(tc => tc.Notes)
                .HasMaxLength(500);

            builder.HasIndex(tc => new { tc.HabitTaskId, tc.UserId, tc.CompletionDate }).IsUnique();
            builder.HasIndex(tc => new { tc.UserId, tc.CompletionDate })
                .HasDatabaseName("IX_TaskCompletion_User_Date");
            builder.HasIndex(tc => new { tc.HabitTaskId, tc.CompletionDate })
                .HasDatabaseName("IX_TaskCompletion_Task_Date");

            builder.HasOne(tc => tc.HabitTask)
                .WithMany(t => t.TaskCompletions)
                .HasForeignKey(tc => tc.HabitTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tc => tc.User)
                .WithMany(u => u.TaskCompletions)
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
