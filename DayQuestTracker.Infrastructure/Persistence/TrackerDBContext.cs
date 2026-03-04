using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Infrastructure.Persistence
{
    public class TrackerDBContext : DbContext
    {
        public TrackerDBContext(DbContextOptions<TrackerDBContext> options)
        : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<HabitTask> Tasks => Set<HabitTask>();
        public DbSet<HabitTaskSchedule> TaskSchedules => Set<HabitTaskSchedule>();
        public DbSet<HabitTaskCompletion> TaskCompletions => Set<HabitTaskCompletion>();
        public DbSet<UserTaskStreak> UserTaskStreaks => Set<UserTaskStreak>();
        public DbSet<DailyScore> DailyScores => Set<DailyScore>();
        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
        public DbSet<XPEvent> XPEvents => Set<XPEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Auto apply all IEntityTypeConfiguration classes in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrackerDBContext).Assembly);
        }
    }
}
