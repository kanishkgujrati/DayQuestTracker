using Microsoft.Extensions.DependencyInjection;

namespace DayQuestTracker.Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services;
        }
    }
}
