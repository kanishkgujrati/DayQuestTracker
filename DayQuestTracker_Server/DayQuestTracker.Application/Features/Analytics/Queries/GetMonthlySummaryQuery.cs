using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public class MonthlySummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalOnceAMonthTasks { get; set; }
        public int CompletedThisMonth { get; set; }
        public int PendingThisMonth { get; set; }
        public double CompletionRate { get; set; }
        public List<MonthlyTaskStatusDto> Tasks { get; set; } = new();
    }

    public class MonthlyTaskStatusDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool CompletedThisMonth { get; set; }
        public DateOnly? CompletedOn { get; set; }
        public int CurrentStreak { get; set; }
        public int XPValue { get; set; }
    }

    public record GetMonthlySummaryQuery(
        Guid UserId,
        int Year,
        int Month) : IRequest<Result<MonthlySummaryDto>>;

    public class GetMonthlySummaryQueryHandler
        : IRequestHandler<GetMonthlySummaryQuery, Result<MonthlySummaryDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetMonthlySummaryQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<MonthlySummaryDto>> Handle(
            GetMonthlySummaryQuery request,
            CancellationToken cancellationToken)
        {
            var firstOfMonth = new DateOnly(request.Year, request.Month, 1);
            var lastOfMonth = new DateOnly(
                request.Year,
                request.Month,
                DateTime.DaysInMonth(request.Year, request.Month));

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.UserId == request.UserId &&
                            t.FrequencyType == FrequencyType.OnceAMonth &&
                            t.DeletedAt == null &&
                            DateOnly.FromDateTime(t.CreatedAt) <= lastOfMonth)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
                return Result<MonthlySummaryDto>.Success(new MonthlySummaryDto
                {
                    Year = request.Year,
                    Month = request.Month,
                    MonthName = firstOfMonth.ToString("MMMM yyyy"),
                    TotalOnceAMonthTasks = 0
                });

            var taskIds = tasks.Select(t => t.Id).ToList();

            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate >= firstOfMonth &&
                             tc.CompletionDate <= lastOfMonth &&
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

                return new MonthlyTaskStatusDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    CategoryName = task.Category?.Name ?? string.Empty,
                    CompletedThisMonth = completion is not null,
                    CompletedOn = completion?.CompletionDate,
                    CurrentStreak = streak?.CurrentStreak ?? 0,
                    XPValue = task.XPValue
                };
            }).ToList();

            var completed = taskStatuses.Count(t => t.CompletedThisMonth);
            var total = taskStatuses.Count;

            return Result<MonthlySummaryDto>.Success(new MonthlySummaryDto
            {
                Year = request.Year,
                Month = request.Month,
                MonthName = firstOfMonth.ToString("MMMM yyyy"),
                TotalOnceAMonthTasks = total,
                CompletedThisMonth = completed,
                PendingThisMonth = total - completed,
                CompletionRate = total > 0
                    ? Math.Round((double)completed / total * 100, 1)
                    : 0,
                Tasks = taskStatuses
            });
        }
    }
}
