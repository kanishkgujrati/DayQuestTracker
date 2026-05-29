using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Application.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Commands
{
    public record DeleteHabitTaskCommand(Guid Id,  Guid UserId) : IRequest<Result<bool>>;

    public class DeleteHabitTaskCommandHandler : IRequestHandler<DeleteHabitTaskCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;
        private readonly DailyScoreService _dailyScoreService;

        public DeleteHabitTaskCommandHandler(ITrackerDbContext context, DailyScoreService dailyScoreService)
        {
            _context = context;
            _dailyScoreService = dailyScoreService;
        }

        public async Task<Result<bool>> Handle(DeleteHabitTaskCommand request,CancellationToken cancellationToken)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == request.Id &&
                                          t.UserId == request.UserId,
                                     cancellationToken);

            if (task is null)
                return Result<bool>.Failure("Task not found.");

            task.DeletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate today's DailyScore to reflect the deleted task
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            await _dailyScoreService.UpsertAsync(request.UserId, today, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
