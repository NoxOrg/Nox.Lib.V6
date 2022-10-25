using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nox.Dynamic.DatabaseProviders;
using Nox.Dynamic.Loaders;
using Nox.Dynamic.OData.Models;
using Nox.Dynamic.OData.Routing;
using Nox.Dynamic.Services;
using System.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Nox.Dynamic.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddNox(this IServiceCollection services)
        {
            services.AddDynamicDefinitionFeature();

            services.AddDynamicODataFeature();

            services.AddHangfireFeature();

            return services;
        }

        public static IServiceCollection AddHangfireFeature(this IServiceCollection services)
        {
            // hangfire feature

            services.AddHangfire( (services,configuration) => ConfigureHangfire(configuration, services));

            services.AddHangfireServer();

            return services;
        }

        private static IServiceCollection AddDynamicDefinitionFeature(this IServiceCollection services)
        {
            services.AddSingleton<ILoaderExecutor, LoaderExecutor>();

            services.AddSingleton<IDynamicService, DynamicService>();

            return services;
        }


        private static IGlobalConfiguration ConfigureHangfire(IGlobalConfiguration configuration, IServiceProvider services)
        {

            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            var model = services.GetRequiredService<DynamicModel>();
            var dbProvider = model.GetDatabaseProvider();

            dbProvider.ConfigureHangfire(configuration);

            model.SetupRecurringLoaderTasks();

            return configuration;
        }

        public static IServiceCollection AddDynamicODataFeature(this IServiceCollection services)
        {
            services.AddControllers().AddOData(options => options
                .Select().Filter().OrderBy().Count().Expand().SkipToken().SetMaxTop(100)
            );

            services.AddSingleton<DynamicModel>();

            services.AddDbContext<DynamicDbContext>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, EntityODataRoutingApplicationModelProvider>());

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<MatcherPolicy, EntityODataRoutingMatcherPolicy>());

            return services;
        }

    }
}
