using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Analytics.Queries
{
    public record GetDailyScoreTrendQuery(Guid UserId,DateOnly StartDate,DateOnly EndDate) : IRequest<Result<List<DailyScoreTrendDto>>>;

    public class GetDailyScoreTrendQueryHandler : IRequestHandler<GetDailyScoreTrendQuery, Result<List<DailyScoreTrendDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetDailyScoreTrendQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<DailyScoreTrendDto>>> Handle(GetDailyScoreTrendQuery request,CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                return Result<List<DailyScoreTrendDto>>
                    .Failure("StartDate cannot be after EndDate.");

            var scores = await _context.DailyScores
                .Where(ds => ds.UserId == request.UserId &&
                             ds.Date >= request.StartDate &&
                             ds.Date <= request.EndDate)
                .OrderBy(ds => ds.Date)
                .Select(ds => new DailyScoreTrendDto
                {
                    Date = ds.Date,
                    Score = ds.Score,
                    CompletedTasks = ds.CompletedTasks,
                    TotalTasks = ds.TotalTasks,
                    XPEarned = ds.XPEarned
                })
                .ToListAsync(cancellationToken);

            // Fill in missing days with zero scores
            // Why: frontend chart needs a data point for every day
            // even days where user had no activity
            var allDays = new List<DailyScoreTrendDto>();
            var current = request.StartDate;

            while (current <= request.EndDate)
            {
                var existing = scores.FirstOrDefault(s => s.Date == current);
                allDays.Add(existing ?? new DailyScoreTrendDto
                {
                    Date = current,
                    Score = 0,
                    CompletedTasks = 0,
                    TotalTasks = 0,
                    XPEarned = 0
                });
                current = current.AddDays(1);
            }

            return Result<List<DailyScoreTrendDto>>.Success(allDays);
        }
    }
}
