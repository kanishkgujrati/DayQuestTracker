using DayQuestTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayQuestTracker.Application.Common.Interfaces
{
    public interface IAuthTokenGeneratorService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
