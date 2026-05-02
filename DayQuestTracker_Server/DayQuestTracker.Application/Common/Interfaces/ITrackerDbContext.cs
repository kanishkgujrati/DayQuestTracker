using DayQuestTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Common.Interfaces
{
    public interface ITrackerDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Category> Categories { get; }
        DbSet<HabitTask> Tasks { get; }
        DbSet<HabitTaskSchedule> TaskSchedules { get; }
        DbSet<HabitTaskCompletion> TaskCompletions { get; }
        DbSet<UserTaskStreak> UserTaskStreaks { get; }
        DbSet<DailyScore> DailyScores { get; }
        DbSet<Achievement> Achievements { get; }
        DbSet<UserAchievement> UserAchievements { get; }
        DbSet<XPEvent> XPEvents { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
