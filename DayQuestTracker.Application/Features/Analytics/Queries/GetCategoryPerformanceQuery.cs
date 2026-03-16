using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{


    public record GetCategoryPerformanceQuery(Guid UserId, DateOnly StartDate, DateOnly EndDate) : IRequest<Result<List<CategoryPerformanceDto>>>;

    public class GetCategoryPerformanceQueryHandler : IRequestHandler<GetCategoryPerformanceQuery, Result<List<CategoryPerformanceDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetCategoryPerformanceQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CategoryPerformanceDto>>> Handle(GetCategoryPerformanceQuery request,CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                return Result<List<CategoryPerformanceDto>>
                    .Failure("StartDate cannot be after EndDate.");

            var categories = await _context.Categories
                .Where(c => c.UserId == request.UserId &&
                            c.DeletedAt == null)
                .ToListAsync(cancellationToken);

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            t.DeletedAt == null)
                .ToListAsync(cancellationToken);

            var taskIds = tasks.Select(t => t.Id).ToList();

            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate >= request.StartDate &&
                             tc.CompletionDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            var streaks = await _context.UserTaskStreaks
                .Where(s => s.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var xpEvents = await _context.XPEvents
                .Where(x => x.UserId == request.UserId &&
                            x.CategoryId != null &&
                            x.TaskCompletionId != null &&
                            _context.TaskCompletions.Any(tc =>
                                tc.Id == x.TaskCompletionId &&
                                tc.CompletionDate >= request.StartDate &&
                                tc.CompletionDate <= request.EndDate))
                .ToListAsync(cancellationToken);

            var result = categories.Select(category =>
            {
                var categoryTasks = tasks
                    .Where(t => t.CategoryId == category.Id)
                    .ToList();

                if (!categoryTasks.Any())
                    return new CategoryPerformanceDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Color = category.Color,
                        TotalTasks = 0,
                        AverageConsistency = 0,
                        TotalXPEarned = 0,
                        BestStreak = 0
                    };

                // Calculate consistency for each task in category
                var consistencies = categoryTasks
                    .Select(task => ConsistencyCalculator.Calculate(
                        task,
                        request.StartDate,
                        request.EndDate,
                        completions.Where(c => c.HabitTaskId == task.Id).ToList()))
                    .ToList();

                var avgConsistency = consistencies.Any()
                    ? Math.Round(consistencies.Average(c => c.ConsistencyPercent), 1)
                    : 0;

                var categoryTaskIds = categoryTasks.Select(t => t.Id).ToList();

                var totalXP = xpEvents
                    .Where(x => x.CategoryId == category.Id &&
                                x.XPAmount > 0)
                    .Sum(x => x.XPAmount);

                var bestStreak = streaks
                    .Where(s => categoryTaskIds.Contains(s.TaskId))
                    .MaxBy(s => s.LongestStreak)?.LongestStreak ?? 0;

                return new CategoryPerformanceDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Color = category.Color,
                    TotalTasks = categoryTasks.Count,
                    AverageConsistency = avgConsistency,
                    TotalXPEarned = totalXP,
                    BestStreak = bestStreak
                };
            })
            .OrderByDescending(c => c.AverageConsistency)
            .ToList();

            return Result<List<CategoryPerformanceDto>>.Success(result);
        }
    }
}