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
    public class XPEventConfiguration : IEntityTypeConfiguration<XPEvent>
    {
        public void Configure(EntityTypeBuilder<XPEvent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.XPAmount).IsRequired();

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_XPEvents_User");
            builder.HasIndex(x => x.CategoryId)
                .HasDatabaseName("IX_XPEvents_Category");

            builder.HasOne(x => x.User)
                .WithMany(u => u.XPEvents)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TaskCompletion)
                .WithMany()
                .HasForeignKey(x => x.TaskCompletionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
