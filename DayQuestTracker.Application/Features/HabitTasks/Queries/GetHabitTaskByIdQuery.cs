using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Queries
{
    public record GetHabitTaskByIdQuery(Guid Id, Guid UserId) : IRequest<Result<HabitTaskDto>>;

    public class GetHabitTaskByIdQueryHandler : IRequestHandler<GetHabitTaskByIdQuery, Result<HabitTaskDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetHabitTaskByIdQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<HabitTaskDto>> Handle(GetHabitTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskSchedules)
                .Where(t => t.Id == request.Id && t.UserId == request.UserId)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (task is null)
                return Result<HabitTaskDto>.Failure("Task not found.");

            return Result<HabitTaskDto>.Success(task);
        }
    }
}
