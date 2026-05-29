using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DayQuestTracker.Application.Common.Services;

namespace DayQuestTracker.Application.Features.Completions.Commands
{
    public record UndoCompletionCommand(Guid CompletionId,Guid UserId) : IRequest<Result<bool>>;

    public class UndoCompletionCommandHandler : IRequestHandler<UndoCompletionCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;
        private readonly DailyScoreService _dailyScoreService;

        public UndoCompletionCommandHandler(ITrackerDbContext context, DailyScoreService dailyScoreService)
        {
            _context = context;
            _dailyScoreService = dailyScoreService;
        }

        public async Task<Result<bool>> Handle(UndoCompletionCommand request, CancellationToken cancellationToken)
        {
            // Fetch completion, validate ownership
            var completion = await _context.TaskCompletions
                .Include(tc => tc.HabitTask)
                .FirstOrDefaultAsync(tc => tc.Id == request.CompletionId &&
                                           tc.UserId == request.UserId,
                                     cancellationToken);

            if (completion is null)
                return Result<bool>.Failure("Completion record not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (completion.CompletionDate < today.AddDays(-7))
                return Result<bool>.Failure("Cannot undo completions older than 7 days.");

            var completionDate = completion.CompletionDate;
            var wasCompleted = completion.Status == CompletionStatus.Completed;

            // Deduct XP if was Completed
            if (wasCompleted)
            {
                var xpToDeduct = completion.HabitTask?.XPValue ?? 0;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user is not null)
                {
                    user.TotalXP = Math.Max(0, user.TotalXP - xpToDeduct);
                }

                // Log negative XP event for audit trail
                _context.XPEvents.Add(new XPEvent
                {
                    UserId = request.UserId,
                    TaskCompletionId = completion.Id,
                    CategoryId = completion.HabitTask?.CategoryId,
                    XPAmount = -xpToDeduct,
                    Reason = XPReason.UndoCompletion
                });
            }

            // Delete completion record
            _context.TaskCompletions.Remove(completion);

            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate streak from scratch
            await RecalculateStreakAsync(completion.HabitTaskId,request.UserId,cancellationToken);

            // Recalculate DailyScore for that date
            await _dailyScoreService.UpsertAsync(request.UserId,completionDate,cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        public async Task RecalculateStreakAsync(Guid taskId,Guid userId,CancellationToken cancellationToken)
        {
            var streak = await _context.UserTaskStreaks.FirstOrDefaultAsync(s => s.TaskId == taskId && s.UserId == userId, cancellationToken);

            if (streak is null) return;

            // Fetch task with schedules for frequency-aware calculation
            var task = await _context.Tasks
                .Include(t => t.TaskSchedules)
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task is null) return;

            // Fetch remaining completed dates after deletion
            var allCompletedDates = await _context.TaskCompletions
                .Where(tc => tc.HabitTaskId == taskId &&
                             tc.UserId == userId &&
                             tc.Status == CompletionStatus.Completed)
                .Select(tc => tc.CompletionDate)
                .ToListAsync(cancellationToken);

            StreakCalculator.Recalculate(streak, task, allCompletedDates);
        }
    }
}
