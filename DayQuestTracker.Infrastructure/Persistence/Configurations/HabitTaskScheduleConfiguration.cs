using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayQuestTracker.Infrastructure.Persistence.Configurations
{
    public class HabitTaskScheduleConfiguration : IEntityTypeConfiguration<HabitTaskSchedule>
    {
        public void Configure(EntityTypeBuilder<HabitTaskSchedule> builder)
        {
            builder.HasKey(ts => ts.Id);

            builder.Property(ts => ts.DayOfWeek).IsRequired();

            builder.HasIndex(ts => new { ts.HabitTaskId, ts.DayOfWeek }).IsUnique();

            builder.HasOne(ts => ts.HabitTask)
                .WithMany(t => t.TaskSchedules)
                .HasForeignKey(ts => ts.HabitTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
