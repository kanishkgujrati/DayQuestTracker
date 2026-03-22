using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
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
            var dayOfWeek = (int)request.Date.DayOfWeek == 0
                ? 6
                : (int)request.Date.DayOfWeek - 1;

            // Fetch all active tasks scheduled for this day
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            t.DeletedAt == null &&
                            (t.FrequencyType == FrequencyType.Daily ||
                             t.TaskSchedules.Any(s => s.DayOfWeek == dayOfWeek)))
                .OrderBy(t => t.Category.Name)
                .ThenBy(t => t.Title)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
                return Result<List<DailyTaskViewDto>>.Success(new List<DailyTaskViewDto>());

            var taskIds = tasks.Select(t => t.Id).ToList();

            // Fetch completions for these tasks on this date — single query
            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate == request.Date)
                .ToListAsync(cancellationToken);

            // Fetch streaks for these tasks
            var streaks = await _context.UserTaskStreaks
                .Where(s => s.UserId == request.UserId &&
                            taskIds.Contains(s.TaskId))
                .ToListAsync(cancellationToken);

            // Combine into daily view
            var result = tasks.Select(task =>
            {
                var completion = completions
                    .FirstOrDefault(c => c.HabitTaskId == task.Id);

                var streak = streaks
                    .FirstOrDefault(s => s.TaskId == task.Id);

                return new DailyTaskViewDto
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
                };
            }).ToList();

            return Result<List<DailyTaskViewDto>>.Success(result);
        }
    }
}
