using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Jobs;

public static class ServiceExtension
{
    public static IServiceCollection AddJobScheduler(this IServiceCollection services)
    {
        services.AddHangfire((serviceProvider, configuration) =>
        {
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            var model = serviceProvider.GetRequiredService<IDynamicModel>();
            var dbProvider = model.GetDatabaseProvider();
            dbProvider.ConfigureJobScheduler(configuration);
            var dynamicService = serviceProvider.GetRequiredService<IDynamicService>();
            dynamicService.SetupRecurringIntegrationTasks();
        });
        services.AddHangfireServer();
        return services;
    }
}