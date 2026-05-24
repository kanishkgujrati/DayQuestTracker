using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.HabitTasks.Queries
{
    public record GetDailyTaskViewQuery(Guid UserId,DateOnly Date) : IRequest<Result<List<DailyTaskViewDto>>>;

    public class GetDailyTaskViewQueryHandler : IRequestHandler<GetDailyTaskViewQuery, Result<List<DailyTaskViewDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetDailyTaskViewQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<DailyTaskViewDto>>> Handle(GetDailyTaskViewQuery request,CancellationToken cancellationToken)
        {
            // Convert date to 0=Mon, 6=Sun format
            var dayOfWeek = (int)request.Date.DayOfWeek == 0 ? 6 : (int)request.Date.DayOfWeek - 1;

            // Get Monday and Sunday of requested date's week
            var dayOfWeekRaw = (int)request.Date.DayOfWeek;
            var daysFromMonday = dayOfWeekRaw == 0 ? 6 : dayOfWeekRaw - 1;
            var monday = request.Date.AddDays(-daysFromMonday);
            var sunday = monday.AddDays(6);

            // Get first and last day of requested date's month
            var firstOfMonth = new DateOnly(request.Date.Year, request.Date.Month, 1);
            var lastOfMonth = new DateOnly(
                request.Date.Year,
                request.Date.Month,
                DateTime.DaysInMonth(request.Date.Year, request.Date.Month));

            // Fetch all active tasks for this user
            var allTasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            t.DeletedAt == null &&
                            DateOnly.FromDateTime(t.CreatedAt) <= request.Date)
                .ToListAsync(cancellationToken);

            // Filter tasks that should appear on this date
            var tasksForDay = allTasks.Where(task =>
                task.FrequencyType == FrequencyType.Daily ||
                (task.FrequencyType == FrequencyType.Weekly &&
                 task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek)) ||
                (task.FrequencyType == FrequencyType.Custom &&
                 task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek)) ||
                task.FrequencyType == FrequencyType.OnceAWeek ||
                task.FrequencyType == FrequencyType.OnceAMonth
            ).ToList();

            if (!tasksForDay.Any())
                return Result<List<DailyTaskViewDto>>.Success(new List<DailyTaskViewDto>());

            var taskIds = tasksForDay.Select(t => t.Id).ToList();

            // Fetch completions for these tasks
            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId))
                .ToListAsync(cancellationToken);

            var streaks = await _context.UserTaskStreaks
                .Where(s => s.UserId == request.UserId &&
                            taskIds.Contains(s.TaskId))
                .ToListAsync(cancellationToken);

            var result = new List<DailyTaskViewDto>();

            foreach (var task in tasksForDay.OrderBy(t => t.Category?.Name).ThenBy(t => t.Title))
            {
                HabitTaskCompletion? completion = null;

                if (task.FrequencyType == FrequencyType.OnceAWeek)
                {
                    // Find completion for this Mon-Sun week
                    completion = completions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate >= monday &&
                        c.CompletionDate <= sunday &&
                        c.Status == CompletionStatus.Completed);

                    // Skip if already completed this week — do not show on dashboard
                    if (completion is not null) continue;
                }
                else if (task.FrequencyType == FrequencyType.OnceAMonth)
                {
                    // Find completion for this calendar month
                    completion = completions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate >= firstOfMonth &&
                        c.CompletionDate <= lastOfMonth &&
                        c.Status == CompletionStatus.Completed);

                    // Skip if already completed this month — do not show on dashboard
                    if (completion is not null) continue;
                }
                else
                {
                    // Daily, Weekly, Custom — find completion for this specific date
                    completion = completions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate == request.Date);
                }

                var streak = streaks.FirstOrDefault(s => s.TaskId == task.Id);

                result.Add(new DailyTaskViewDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    CategoryName = task.Category?.Name ?? string.Empty,
                    CategoryColor = task.Category?.Color ?? string.Empty,
                    Difficulty = task.Difficulty,
                    XPValue = task.XPValue,
                    FrequencyType = task.FrequencyType,
                    CompletionId = completion?.Id,
                    Status = completion?.Status,
                    Notes = completion?.Notes,
                    CurrentStreak = streak?.CurrentStreak ?? 0
                });
            }

            return Result<List<DailyTaskViewDto>>.Success(result);
        }

    }
}
