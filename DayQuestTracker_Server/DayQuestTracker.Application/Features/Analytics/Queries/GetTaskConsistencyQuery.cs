using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{

    public record GetTaskConsistencyQuery(Guid UserId,DateOnly StartDate,DateOnly EndDate,Guid? CategoryId = null) : IRequest<Result<List<TaskConsistencyDto>>>;

    public class GetTaskConsistencyQueryHandler : IRequestHandler<GetTaskConsistencyQuery, Result<List<TaskConsistencyDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetTaskConsistencyQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TaskConsistencyDto>>> Handle(GetTaskConsistencyQuery request,CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                return Result<List<TaskConsistencyDto>>
                    .Failure("StartDate cannot be after EndDate.");

            // Fetch all active tasks for this user
            var tasksQuery = _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            t.DeletedAt == null);

            if (request.CategoryId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CategoryId == request.CategoryId.Value);

            var tasks = await tasksQuery
                .OrderBy(t => t.Category.Name)
                .ThenBy(t => t.Title)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
                return Result<List<TaskConsistencyDto>>.Success(new List<TaskConsistencyDto>());

            var taskIds = tasks.Select(t => t.Id).ToList();

            // Fetch all completions for these tasks in the date range — single query
            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate >= request.StartDate &&
                             tc.CompletionDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            // Calculate consistency for each task using shared calculator
            var result = tasks
                .Select(task => ConsistencyCalculator.Calculate(
                    task,
                    request.StartDate,
                    request.EndDate,
                    completions.Where(c => c.HabitTaskId == task.Id).ToList()))
                .ToList();

            return Result<List<TaskConsistencyDto>>.Success(result);
        }
    }
}
