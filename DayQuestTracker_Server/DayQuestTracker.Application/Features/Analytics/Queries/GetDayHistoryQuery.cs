using MediatR;
using Microsoft.EntityFrameworkCore;
using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Enums;
using DayQuestTracker.Domain.DTOs;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public record GetDayHistoryQuery(Guid UserId, DateOnly Date) : IRequest<Result<DayHistoryDto>>;

    public class GetDayHistoryQueryHandler : IRequestHandler<GetDayHistoryQuery, Result<DayHistoryDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetDayHistoryQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DayHistoryDto>> Handle(GetDayHistoryQuery request, CancellationToken cancellationToken)
        {
            var date = request.Date;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // No future dates
            if (date > today)
                return Result<DayHistoryDto>.Failure("Cannot view history for future dates.");

            var dayOfWeek = (int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1;
        
            // Week boundaries for OnceAWeek check
            var dayOfWeekRaw = (int)date.DayOfWeek;
            var daysFromMonday = dayOfWeekRaw == 0 ? 6 : dayOfWeekRaw - 1;
            var monday = date.AddDays(-daysFromMonday);
            var sunday = monday.AddDays(6);

                // Month boundaries for OnceAMonth check
            var firstOfMonth = new DateOnly(date.Year, date.Month, 1);
            var lastOfMonth = new DateOnly(
                date.Year,
                date.Month,
                DateTime.DaysInMonth(date.Year, date.Month));
            // Fetch all tasks that existed on this date
            var allTasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            DateOnly.FromDateTime(t.CreatedAt) <= date &&
                            (t.DeletedAt == null ||
                             DateOnly.FromDateTime(t.DeletedAt.Value) > date))
                .ToListAsync(cancellationToken);

            // Filter tasks scheduled for this date
            var scheduledTasks = allTasks.Where(task =>
                task.FrequencyType == FrequencyType.Daily ||
                (task.FrequencyType == FrequencyType.Weekly &&
                 task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek)) ||
                (task.FrequencyType == FrequencyType.Custom &&
                 task.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek)) ||
                task.FrequencyType == FrequencyType.OnceAWeek ||
                task.FrequencyType == FrequencyType.OnceAMonth
            ).ToList();

            if (!scheduledTasks.Any())
                return Result<DayHistoryDto>.Success(new DayHistoryDto
                {
                    Date = date,
                    DayName = date.ToString("dddd"),
                    Score = 0,
                    TotalTasks = 0
                });

            var taskIds = scheduledTasks.Select(t => t.Id).ToList();

            // Fetch all completions needed
            var allCompletions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId))
                .ToListAsync(cancellationToken);

            // Get daily score if exists
            var dailyScore = await _context.DailyScores
                .FirstOrDefaultAsync(ds => ds.UserId == request.UserId &&
                                           ds.Date == date,
                                     cancellationToken);

                var historyTasks = new List<DayHistoryTaskDto>();

            foreach (var task in scheduledTasks.OrderBy(t => t.Category?.Name).ThenBy(t => t.Title))
            {
                DayTaskStatus status;
                string? notes = null;
                int xpAwarded = 0;
                if (task.FrequencyType == FrequencyType.OnceAWeek)
                {
                    var completion = allCompletions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate >= monday &&
                        c.CompletionDate <= sunday);

                    if (completion == null)
                        status = DayTaskStatus.Missed;
                    else if (completion.Status == CompletionStatus.Completed)
                    {
                        status = DayTaskStatus.Completed;
                        notes = completion.Notes;
                        xpAwarded = task.XPValue;
                    }
                    else
                    {
                        status = DayTaskStatus.Skipped;
                        notes = completion.Notes;
                    }
                }
                else if (task.FrequencyType == FrequencyType.OnceAMonth)
                {
                    var completion = allCompletions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate >= firstOfMonth &&
                            c.CompletionDate <= lastOfMonth);

                    if (completion == null)
                        status = DayTaskStatus.Missed;
                    else if (completion.Status == CompletionStatus.Completed)
                    {
                        status = DayTaskStatus.Completed;
                        notes = completion.Notes;
                        xpAwarded = task.XPValue;
                    }
                    else
                    {
                        status = DayTaskStatus.Skipped;
                        notes = completion.Notes;
                    }
                }
                else
                {
                    var completion = allCompletions.FirstOrDefault(c =>
                        c.HabitTaskId == task.Id &&
                        c.CompletionDate == date);
                
                    if (completion == null)
                        status = DayTaskStatus.Missed;
                    else if (completion.Status == CompletionStatus.Completed)
                    {
                        status = DayTaskStatus.Completed;
                        notes = completion.Notes;
                        xpAwarded = task.XPValue;
                    }
                    else
                    {
                        status = DayTaskStatus.Skipped;
                        notes = completion.Notes;
                    }
                }

                historyTasks.Add(new DayHistoryTaskDto
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    CategoryName = task.Category?.Name ?? string.Empty,
                    CategoryColor = task.Category?.Color ?? string.Empty,
                    Difficulty = task.Difficulty,
                    XPValue = task.XPValue,
                    FrequencyType = task.FrequencyType,
                    Status = status,
                    Notes = notes,
                    XPAwarded = xpAwarded
                });
            }

            var completedCount = historyTasks.Count(t =>
                t.Status == DayTaskStatus.Completed);
            var skippedCount = historyTasks.Count(t =>
                t.Status == DayTaskStatus.Skipped);
            var missedCount = historyTasks.Count(t =>
                t.Status == DayTaskStatus.Missed);

            return Result<DayHistoryDto>.Success(new DayHistoryDto
            {
                Date = date,
                DayName = date.DayOfWeek.ToString(),
                Score = dailyScore?.Score ?? 0,
                TotalTasks = historyTasks.Count,
                CompletedCount = completedCount,
                SkippedCount = skippedCount,
                MissedCount = missedCount,
                XPEarned = dailyScore?.XPEarned ?? 0,
                Tasks = historyTasks
            });
        }
    }
}
