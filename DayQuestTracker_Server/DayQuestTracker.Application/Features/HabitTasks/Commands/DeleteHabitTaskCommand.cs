using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Tasks.Commands
{
    public record DeleteHabitTaskCommand(Guid Id,  Guid UserId) : IRequest<Result<bool>>;

    public class DeleteHabitTaskCommandHandler : IRequestHandler<DeleteHabitTaskCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;

        public DeleteHabitTaskCommandHandler(ITrackerDbContext context)
        {
            _context = context;
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

            return Result<bool>.Success(true);
        }
    }
}
