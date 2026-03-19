using BCrypt.Net;
using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.UserProfile.Commands
{
    public record ChangePasswordCommand(Guid UserId,string CurrentPassword,string NewPassword,string ConfirmNewPassword) : IRequest<Result<bool>>;

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;

        public ChangePasswordCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ChangePasswordCommand request,CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId &&
                                          u.IsActive,
                                     cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found.");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect.");

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                return Result<bool>.Failure("New password must be different from current password.");

            // Hash and save new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate refresh token — force re-login on all devices
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}
