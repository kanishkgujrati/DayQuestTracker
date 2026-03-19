using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.UserProfile.Commands
{
    public record UpdateProfileCommand(Guid UserId, string? Username, string? Timezone) : IRequest<Result<UserProfileDto>>;

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly ITrackerDbContext _context;

        public UpdateProfileCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request,CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId &&
                                          u.IsActive,
                                     cancellationToken);

            if (user is null)
                return Result<UserProfileDto>.Failure("User not found.");

            // Username update — business logic check for uniqueness
            if (request.Username is not null)
            {
                var usernameTaken = await _context.Users
                    .AnyAsync(u => u.Username == request.Username.Trim() &&
                                   u.Id != request.UserId,
                              cancellationToken);

                if (usernameTaken)
                    return Result<UserProfileDto>.Failure("Username is already taken.");

                user.Username = request.Username.Trim();
            }

            if (request.Timezone is not null)
                user.Timezone = request.Timezone;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<UserProfileDto>.Success(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Timezone = user.Timezone,
                TotalXP = user.TotalXP,
                Level = user.Level,
                CreatedAt = user.CreatedAt
            });
        }
    }
}
