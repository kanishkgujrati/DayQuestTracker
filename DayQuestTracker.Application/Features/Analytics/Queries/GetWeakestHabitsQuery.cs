using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public record GetWeakestHabitsQuery(Guid UserId,DateOnly StartDate,DateOnly EndDate,int TopN = 5) : IRequest<Result<List<TaskConsistencyDto>>>;

    public class GetWeakestHabitsQueryHandler : IRequestHandler<GetWeakestHabitsQuery, Result<List<TaskConsistencyDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetWeakestHabitsQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TaskConsistencyDto>>> Handle(GetWeakestHabitsQuery request,CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                return Result<List<TaskConsistencyDto>>
                    .Failure("StartDate cannot be after EndDate.");

            if (request.TopN < 1 || request.TopN > 20)
                return Result<List<TaskConsistencyDto>>
                    .Failure("TopN must be between 1 and 20.");

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId &&
                            t.DeletedAt == null)
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
                return Result<List<TaskConsistencyDto>>.Success(new List<TaskConsistencyDto>());

            var taskIds = tasks.Select(t => t.Id).ToList();

            var completions = await _context.TaskCompletions
                .Where(tc => tc.UserId == request.UserId &&
                             taskIds.Contains(tc.HabitTaskId) &&
                             tc.CompletionDate >= request.StartDate &&
                             tc.CompletionDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            // Reuse same calculator — sort by lowest consistency
            var result = tasks
                .Select(task => ConsistencyCalculator.Calculate(
                    task,
                    request.StartDate,
                    request.EndDate,
                    completions.Where(c => c.HabitTaskId == task.Id).ToList()))
                .Where(r => r.TotalScheduledDays > 0) // exclude tasks not scheduled in range
                .OrderBy(r => r.ConsistencyPercent)   // lowest first
                .Take(request.TopN)
                .ToList();

            return Result<List<TaskConsistencyDto>>.Success(result);
        }
    }
}
