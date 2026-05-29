using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DayQuestTracker.Application.Common.Services;

namespace DayQuestTracker.Application.Features.Completions.Commands
{
    public record LogCompletionCommand(Guid TaskId, Guid UserId, DateOnly CompletionDate, CompletionStatus Status, string? Notes) : IRequest<Result<TaskCompletionDto>>;

    public class LogCompletionCommandHandler : IRequestHandler<LogCompletionCommand, Result<TaskCompletionDto>>
    {
        private readonly ITrackerDbContext _context;
        private readonly DailyScoreService _dailyScoreService;

        public LogCompletionCommandHandler(ITrackerDbContext context, DailyScoreService dailyScoreService)
        {
            _context = context;
            _dailyScoreService = dailyScoreService;
        }
        public async Task<Result<TaskCompletionDto>> Handle(LogCompletionCommand request, CancellationToken cancellationToken)
        {
            // Fetch task WITH schedules — needed for frequency-aware streak calculation
            var task = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId &&
                                          t.UserId == request.UserId,
                                     cancellationToken);

            if (task is null)
                return Result<TaskCompletionDto>.Failure("Task not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (request.CompletionDate > today)
                return Result<TaskCompletionDto>.Failure("Cannot log completions for future dates.");

            if (request.CompletionDate < today.AddDays(-7))
                return Result<TaskCompletionDto>.Failure("Cannot log completions older than 7 days.");

            var existingCompletion = await _context.TaskCompletions
                .FirstOrDefaultAsync(tc => tc.HabitTaskId == request.TaskId &&
                                           tc.UserId == request.UserId &&
                                           tc.CompletionDate == request.CompletionDate,
                                     cancellationToken);

            if (existingCompletion is not null)
                return Result<TaskCompletionDto>.Failure(
                    "A completion record already exists for this task on this date.");

            var completion = new HabitTaskCompletion
            {
                HabitTaskId = request.TaskId,
                UserId = request.UserId,
                CompletionDate = request.CompletionDate,
                Status = request.Status,
                Notes = request.Notes
            };

            _context.TaskCompletions.Add(completion);

            var xpAwarded = 0;

            if (request.Status == CompletionStatus.Completed)
            {
                xpAwarded = task.XPValue;

                _context.XPEvents.Add(new XPEvent
                {
                    UserId = request.UserId,
                    TaskCompletionId = completion.Id,
                    CategoryId = task.CategoryId,
                    XPAmount = xpAwarded,
                    Reason = XPReason.TaskCompletion,
                    CreatedAt = DateTime.UtcNow
                });

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user is not null)
                    user.TotalXP += xpAwarded;
            }

            // First save — commit completion to DB before streak recalculation
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch all completed dates AFTER save
            var allCompletedDates = await _context.TaskCompletions
                .Where(tc => tc.HabitTaskId == request.TaskId &&
                             tc.UserId == request.UserId &&
                             tc.Status == CompletionStatus.Completed)
                .Select(tc => tc.CompletionDate)
                .ToListAsync(cancellationToken);

            // Recalculate streak using frequency-aware logic
            var streak = await _context.UserTaskStreaks
                .FirstOrDefaultAsync(s => s.TaskId == request.TaskId &&
                                          s.UserId == request.UserId,
                                     cancellationToken);

            if (streak is not null)
            {
                StreakCalculator.Recalculate(streak, task, allCompletedDates, request.Status);
            }

            await _dailyScoreService.UpsertAsync(request.UserId,request.CompletionDate,cancellationToken);

            // Second save — persist streak and daily score updates
            await _context.SaveChangesAsync(cancellationToken);

            return Result<TaskCompletionDto>.Success(new TaskCompletionDto
            {
                Id = completion.Id,
                TaskId = completion.HabitTaskId,
                TaskTitle = task.Title,
                CategoryName = task.Category?.Name ?? string.Empty,
                CompletionDate = completion.CompletionDate,
                Status = completion.Status,
                Notes = completion.Notes,
                XPAwarded = xpAwarded,
                CreatedAt = completion.CreatedAt
            });
        }

        private async Task<HabitTaskCompletion?> GetExistingCompletion(Guid taskId, Guid userId, DateOnly date, FrequencyType frequencyType, CancellationToken cancellationToken)
        {
            if (frequencyType == FrequencyType.OnceAWeek)
            {
                // Check if completed any day this Mon-Sun week
                var monday = GetMondayOfWeek(date);
                var sunday = monday.AddDays(6);

                return await _context.TaskCompletions
                    .FirstOrDefaultAsync(tc =>
                        tc.HabitTaskId == taskId &&
                        tc.UserId == userId &&
                        tc.CompletionDate >= monday &&
                        tc.CompletionDate <= sunday &&
                        tc.Status == CompletionStatus.Completed,
                        cancellationToken);
            }

            if (frequencyType == FrequencyType.OnceAMonth)
            {
                // Check if completed any day this calendar month
                var firstOfMonth = new DateOnly(date.Year, date.Month, 1);
                var lastOfMonth = new DateOnly(
                    date.Year,
                    date.Month,
                    DateTime.DaysInMonth(date.Year, date.Month));

                return await _context.TaskCompletions
                    .FirstOrDefaultAsync(tc =>
                        tc.HabitTaskId == taskId &&
                        tc.UserId == userId &&
                        tc.CompletionDate >= firstOfMonth &&
                        tc.CompletionDate <= lastOfMonth &&
                        tc.Status == CompletionStatus.Completed,
                        cancellationToken);
            }

            // Daily, Weekly, Custom — one per date
            return await _context.TaskCompletions
                .FirstOrDefaultAsync(tc =>
                    tc.HabitTaskId == taskId &&
                    tc.UserId == userId &&
                    tc.CompletionDate == date,
                    cancellationToken);
        }

        private static string GetDuplicateMessage(FrequencyType frequencyType) =>
            frequencyType switch
            {
                FrequencyType.OnceAWeek =>
                    "This task has already been completed this week.",
                FrequencyType.OnceAMonth =>
                    "This task has already been completed this month.",
                _ => "A completion record already exists for this task on this date."
            };

        private static DateOnly GetMondayOfWeek(DateOnly date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            return date.AddDays(-daysFromMonday);
        }
    }
}