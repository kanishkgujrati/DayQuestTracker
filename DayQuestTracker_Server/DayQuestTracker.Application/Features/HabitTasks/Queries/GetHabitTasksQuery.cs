using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Queries
{
    public record GetHabitTasksQuery(Guid UserId, Guid? CategoryId = null) : IRequest<Result<List<HabitTaskDto>>>;

    public class GetHabitTasksQueryHandler : IRequestHandler<GetHabitTasksQuery, Result<List<HabitTaskDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetHabitTasksQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<HabitTaskDto>>> Handle(GetHabitTasksQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.UserId == request.UserId);

            // Optional filter by category
            if (request.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == request.CategoryId.Value);

            var tasks = await query
                .OrderBy(t => t.Category.Name)
                .ThenBy(t => t.Title)
                .Select(t => new HabitTaskDto
                {
                    Id = t.Id,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    Title = t.Title,
                    Description = t.Description,
                    Difficulty = t.Difficulty,
                    FrequencyType = t.FrequencyType,
                    TargetPerWeek = t.TargetPerWeek,
                    ScheduledDays = t.TaskSchedules.Select(s => s.DayOfWeek).ToList(),
                    XPValue = t.XPValue,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<HabitTaskDto>>.Success(tasks);
        }
    }
}
