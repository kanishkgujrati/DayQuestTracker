using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Infrastructure.HangfireJobs;
using DayQuestTracker.Infrastructure.Persistence;
using DayQuestTracker.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DayQuestTracker.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthTokenGeneratorService, AuthTokenGeneratorService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<StreakResetJob>();

            services.AddScoped<ITrackerDbContext>(
                provider => provider.GetRequiredService<TrackerDBContext>());

            //Hangfire
            services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection"))));

            services.AddHangfireServer();

            return services;
        }
    }
}
