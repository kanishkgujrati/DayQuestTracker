using Microsoft.Extensions.DependencyInjection;

namespace DayQuestTracker.Application
{
    public sealed class AssemblyReference { }
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
            
            return services;
        }
    }
}
