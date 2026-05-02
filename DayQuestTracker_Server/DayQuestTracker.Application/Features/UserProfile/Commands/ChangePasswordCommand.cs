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
        private readonly IPasswordHasher _passwordHasher;

        public ChangePasswordCommandHandler(ITrackerDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<bool>> Handle(ChangePasswordCommand request,CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId &&
                                          u.IsActive,
                                     cancellationToken);

            if (user is null)
                return Result<bool>.Failure("User not found.");

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect.");

            if (_passwordHasher.Verify(request.NewPassword, user.PasswordHash))
                return Result<bool>.Failure("New password must be different from current password.");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}
