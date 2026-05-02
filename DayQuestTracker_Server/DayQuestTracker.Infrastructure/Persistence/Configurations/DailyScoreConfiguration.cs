using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayQuestTracker.Infrastructure.Persistence.Configurations
{
    public class DailyScoreConfiguration : IEntityTypeConfiguration<DailyScore>
    {
        public void Configure(EntityTypeBuilder<DailyScore> builder)
        {
            builder.HasKey(ds => ds.Id);

            builder.HasIndex(ds => new { ds.UserId, ds.Date }).IsUnique();
            builder.HasIndex(ds => new { ds.UserId, ds.Date })
                .HasDatabaseName("IX_DailyScores_User_Date");

            builder.HasOne(ds => ds.User)
                .WithMany(u => u.DailyScores)
                .HasForeignKey(ds => ds.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
