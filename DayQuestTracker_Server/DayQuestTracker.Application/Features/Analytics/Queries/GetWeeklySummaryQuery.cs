using MediatR;
using Microsoft.EntityFrameworkCore;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Domain.Enums;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public class WeeklySummaryDto
    {
        public DateOnly WeekStart { get; set; }
        public DateOnly WeekEnd { get; set; }
        public int TotalOnceAWeekTasks { get; set; }
        public int CompletedThisWeek { get; set; }
        public int PendingThisWeek { get; set; }
        public double CompletionRate { get; set; }
        public List<WeeklyTaskStatusDto> Tasks { get; set; } = new();
    }

    public class WeeklyTaskStatusDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool CompletedThisWeek { get; set; }
        public DateOnly? CompletedOn { get; set; }
        public int CurrentStreak { get; set; }
        public int XPValue { get; set; }
    }

    public record GetWeeklySummaryQuery(Guid UserId, DateOnly Date) : IRequest<Result<WeeklySummaryDto>>;

    public class GetWeeklySummaryQueryHandler
        : IRequestHandler<GetWeeklySummaryQuery, Result<WeeklySummaryDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetWeeklySummaryQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<WeeklySummaryDto>> Handle(
            GetWeeklySummaryQuery request,
            CancellationToken cancellationToken)
        {
            // Calculate Mon-Sun week boundaries
            var dayOfWeek = (int)request.Date.DayOfWeek;
            var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            var monday = request.Date.AddDays(-daysFromMonday);
            var sunday = monday.AddDays(6);

            // Get all OnceAWeek tasks for this user
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.UserId == request.UserId &&
                            t.FrequencyType == FrequencyType.OnceAWeek &&
                            t.DeletedAt == null &&
                            DateOnly.FromDateTime(t.CreatedAt) <= sunday)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
                return Result<WeeklySummaryDto>.Success(new WeeklySummaryDto
                {
                    WeekStart = monday,
                    WeekEnd = sunday,
                    TotalOnceAWeekTasks = 0
                });

            var taskIds = tasks.Select(t => t.Id).ToList();

            // Get completions for this week
            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate >= monday &&
                             tc.CompletionDate <= sunday &&
                             tc.Status == CompletionStatus.Completed)
                .ToListAsync(cancellationToken);

            var streaks = await _context.UserTaskStreaks
                .Where(s => s.UserId == request.UserId &&
                            taskIds.Contains(s.TaskId))
                .ToListAsync(cancellationToken);

            var taskStatuses = tasks.Select(task =>
            {
                var completion = completions
                    .FirstOrDefault(c => c.HabitTaskId == task.Id);
                var streak = streaks
                    .FirstOrDefault(s => s.TaskId == task.Id);

                return new WeeklyTaskStatusDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    CategoryName = task.Category?.Name ?? string.Empty,
                    CompletedThisWeek = completion is not null,
                    CompletedOn = completion?.CompletionDate,
                    CurrentStreak = streak?.CurrentStreak ?? 0,
                    XPValue = task.XPValue
                };
            }).ToList();

            var completed = taskStatuses.Count(t => t.CompletedThisWeek);
            var total = taskStatuses.Count;

            return Result<WeeklySummaryDto>.Success(new WeeklySummaryDto
            {
                WeekStart = monday,
                WeekEnd = sunday,
                TotalOnceAWeekTasks = total,
                CompletedThisWeek = completed,
                PendingThisWeek = total - completed,
                CompletionRate = total > 0
                    ? Math.Round((double)completed / total * 100, 1)
                    : 0,
                Tasks = taskStatuses
            });
        }
    }
}