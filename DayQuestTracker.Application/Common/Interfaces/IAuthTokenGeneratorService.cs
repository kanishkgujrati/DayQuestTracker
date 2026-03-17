using DayQuestTracker.Domain.Entities;

namespace DayQuestTracker.Application.Common.Interfaces
{
    public interface IAuthTokenGeneratorService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
