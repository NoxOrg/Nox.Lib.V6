using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nox.Dynamic.Configuration;
using Nox.Dynamic.Loaders;
using Nox.Dynamic.MessageBus;
using Nox.Dynamic.OData.Models;
using Nox.Dynamic.OData.Routing;
using Nox.Dynamic.Services;
using System.Reflection;

namespace Nox.Dynamic.Extensions
{
    public static class ServiceCollectionExtensions
    {

        private static readonly IConfiguration? _configuration = ConfigurationHelper.GetNoxConfiguration();

        public static IServiceCollection AddNox(this IServiceCollection services)
        {
            if (_configuration == null)
            {
                throw new ConfigurationException("Could not load Nox configuration.");
            }

            services.AddDynamicDefinitionFeature();

            services.AddMessageBusFeature(_configuration);

            services.AddDynamicODataFeature();

            services.AddHangfireFeature();

            return services;
        }

        public static IServiceCollection AddDynamicDefinitionFeature(this IServiceCollection services)
        {
            services.AddSingleton<ILoaderExecutor, LoaderExecutor>();

            services.AddSingleton<IDynamicService, DynamicService>();

            return services;
        }

        public static IServiceCollection RegisterNoxConsumers(this IServiceCollection services, IList<INoxConsumer> consumers)
        {
            services.AddSingleton(consumers);

            return services;
        }
    

        public static IServiceCollection AddMessageBusFeature(this IServiceCollection services, 
            IConfiguration config, bool isServer = true)
        {
            var provider = config["ServiceMessageBusProvider"];
            var connectionString = config["ServiceMessageBusConnectionString"];
            var connectionVariable = config["ServiceMessageBusConnectionVariable"];

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = config[connectionVariable];
            }

            services.AddMassTransit(mt =>
            {
                mt.SetKebabCaseEndpointNameFormatter();
                
                // By default, sagas are in-memory, but should be changed to a durable
                // saga repository.
                mt.SetInMemorySagaRepositoryProvider();

                if (isServer)
                {
                    var noxAssembly = Assembly.GetExecutingAssembly();
                    mt.AddConsumers(noxAssembly);
                    mt.AddSagaStateMachines(noxAssembly);
                    mt.AddSagas(noxAssembly);
                    mt.AddActivities(noxAssembly);
                }
                else
                {
                    var entryAssembly = Assembly.GetEntryAssembly();
                    mt.AddConsumers(entryAssembly);
                    mt.AddSagaStateMachines(entryAssembly);
                    mt.AddSagas(entryAssembly);
                    mt.AddActivities(entryAssembly);
                }

                switch (provider)
                {
                    case "rabbitmq":
                        mt.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(connectionString);
                            cfg.ConfigureEndpoints(context);
                        });
                        break;

                    case "azureservicebus":
                        mt.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(connectionString);
                            cfg.ConfigureEndpoints(context);
                        });
                        break;
                }
            });

            if (isServer)
            {
                services.AddHostedService<HeartbeatWorker>();
            }

            return services;
        }

        static IServiceCollection AddDynamicODataFeature(this IServiceCollection services)
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

        public static IServiceCollection AddHangfireFeature(this IServiceCollection services)
        {
            // hangfire feature

            services.AddHangfire((services, configuration) => ConfigureHangfire(configuration, services));

            services.AddHangfireServer();

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

            model.
                SetupRecurringLoaderTasks();

            return configuration;
        }
    }
}
