using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.UserProfile.Queries
{
    public record GetProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetProfileQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request,CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId &&
                                          u.IsActive,
                                     cancellationToken);

            if (user is null)
                return Result<UserProfileDto>.Failure("User not found.");

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
