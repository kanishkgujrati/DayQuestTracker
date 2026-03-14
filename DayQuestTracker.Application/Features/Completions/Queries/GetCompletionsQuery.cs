using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Completions.Queries
{
    public record GetCompletionsQuery(Guid UserId,DateOnly StartDate,DateOnly EndDate,Guid? TaskId = null) : IRequest<Result<List<DailyCompletionSummaryDto>>>;

    public class GetCompletionsQueryHandler : IRequestHandler<GetCompletionsQuery, Result<List<DailyCompletionSummaryDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetCompletionsQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<DailyCompletionSummaryDto>>> Handle(GetCompletionsQuery request,CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                return Result<List<DailyCompletionSummaryDto>>.Failure("StartDate cannot be after EndDate.");

            var query = _context.TaskCompletions
                .Include(tc => tc.HabitTask)
                .ThenInclude(t => t.Category)
                .Where(tc => tc.UserId == request.UserId &&
                             tc.CompletionDate >= request.StartDate &&
                             tc.CompletionDate <= request.EndDate);

            if (request.TaskId.HasValue)
                query = query.Where(tc => tc.HabitTaskId == request.TaskId.Value);

            var completions = await query
                .OrderBy(tc => tc.CompletionDate)
                .ThenBy(tc => tc.HabitTask.Title)
                .ToListAsync(cancellationToken);

            // Fetch daily scores for the date range
            var dailyScores = await _context.DailyScores
                .Where(ds => ds.UserId == request.UserId &&
                             ds.Date >= request.StartDate &&
                             ds.Date <= request.EndDate)
                .ToListAsync(cancellationToken);

            // Group completions by date
            var grouped = completions
                .GroupBy(tc => tc.CompletionDate)
                .Select(g =>
                {
                    var dailyScore = dailyScores
                        .FirstOrDefault(ds => ds.Date == g.Key);

                    return new DailyCompletionSummaryDto
                    {
                        Date = g.Key,
                        TotalTasks = dailyScore?.TotalTasks ?? 0,
                        CompletedTasks = g.Count(c => c.Status == CompletionStatus.Completed),
                        SkippedTasks = g.Count(c => c.Status == CompletionStatus.Skipped),
                        Score = dailyScore?.Score ?? 0,
                        XPEarned = dailyScore?.XPEarned ?? 0,
                        Completions = g.Select(tc => new TaskCompletionDto
                        {
                            Id = tc.Id,
                            TaskId = tc.HabitTaskId,
                            TaskTitle = tc.HabitTask?.Title ?? string.Empty,
                            CategoryName = tc.HabitTask?.Category?.Name ?? string.Empty,
                            CompletionDate = tc.CompletionDate,
                            Status = tc.Status,
                            Notes = tc.Notes,
                            XPAwarded = tc.Status == CompletionStatus.Completed
                                ? tc.HabitTask?.XPValue ?? 0
                                : 0,
                            CreatedAt = tc.CreatedAt
                        }).ToList()
                    };
                })
                .OrderBy(d => d.Date)
                .ToList();

            return Result<List<DailyCompletionSummaryDto>>.Success(grouped);
        }
    }
}
