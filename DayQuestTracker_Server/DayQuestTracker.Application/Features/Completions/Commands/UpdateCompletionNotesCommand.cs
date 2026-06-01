using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Completions.Commands
{
    public record UpdateCompletionNotesCommand(Guid CompletionId, Guid UserId, string? Notes) : IRequest<Result<bool>>;

    public class UpdateCompletionNotesCommandHandler
        : IRequestHandler<UpdateCompletionNotesCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;

        public UpdateCompletionNotesCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateCompletionNotesCommand request, CancellationToken cancellationToken)
        {
            var completion = await _context.TaskCompletions
                .FirstOrDefaultAsync(tc => tc.Id == request.CompletionId &&
                                           tc.UserId == request.UserId,
                                     cancellationToken);

            if (completion is null)
                return Result<bool>.Failure("Completion not found.");

            // Empty string clears the note
            completion.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

            completion.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
