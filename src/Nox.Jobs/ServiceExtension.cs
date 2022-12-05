using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

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
            dynamicService.SetupRecurringLoaderTasks();
        });
        services.AddHangfireServer();
        return services;
    }
}