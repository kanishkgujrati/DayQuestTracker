using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.UserProfile.Commands;

public record UpdateProfilePhotoCommand(Guid UserId, string PhotoUrl) : IRequest<Result<bool>>;

public class UpdateProfilePhotoCommandHandler : IRequestHandler<UpdateProfilePhotoCommand, Result<bool>>
{
    private readonly ITrackerDbContext _context;

    public UpdateProfilePhotoCommandHandler(ITrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId &&
                                      u.IsActive,
                                 cancellationToken);

        if (user is null)
            return Result<bool>.Failure("User not found.");

        user.ProfilePhotoUrl = request.PhotoUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}