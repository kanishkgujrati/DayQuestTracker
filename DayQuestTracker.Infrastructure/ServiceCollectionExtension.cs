using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Infrastructure.Persistence;
using DayQuestTracker.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DayQuestTracker.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthTokenGeneratorService, AuthTokenGeneratorService>();

            services.AddScoped<ITrackerDbContext>(
                provider => provider.GetRequiredService<TrackerDBContext>());

            return services;
        }
    }
}
