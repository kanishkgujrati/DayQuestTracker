using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public record GetStreakSummaryQuery(Guid UserId) : IRequest<Result<List<TaskStreakSummaryDto>>>;

    public class GetStreakSummaryQueryHandler : IRequestHandler<GetStreakSummaryQuery, Result<List<TaskStreakSummaryDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetStreakSummaryQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TaskStreakSummaryDto>>> Handle(GetStreakSummaryQuery request,CancellationToken cancellationToken)
        {
            var streaks = await _context.UserTaskStreaks
                .Include(s => s.Task)
                    .ThenInclude(t => t.Category)
                .Where(s => s.UserId == request.UserId &&
                            s.Task.DeletedAt == null)
                .OrderByDescending(s => s.CurrentStreak)
                .Select(s => new TaskStreakSummaryDto
                {
                    TaskId = s.TaskId,
                    TaskTitle = s.Task.Title,
                    CategoryName = s.Task.Category.Name,
                    CurrentStreak = s.CurrentStreak,
                    LongestStreak = s.LongestStreak,
                    LastCompletedDate = s.LastCompletedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<TaskStreakSummaryDto>>.Success(streaks);
        }
    }
}
