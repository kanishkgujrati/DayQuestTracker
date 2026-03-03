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
    public class HabitTaskConfiguration : IEntityTypeConfiguration<HabitTask>
    {
        public void Configure(EntityTypeBuilder<HabitTask> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.Difficulty)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(t => t.FrequencyType)
                .IsRequired()
                .HasConversion<string>();  // Store enum as string in DB

            // XPValue is computed — never mapped to a column
            builder.Ignore(t => t.XPValue);

            builder.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.HabitTasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(t => t.DeletedAt == null);
        }
    }
}
