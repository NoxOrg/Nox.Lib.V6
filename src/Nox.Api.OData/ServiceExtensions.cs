using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nox.Api.OData.Routing;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Entity.Commands;
using System.Reflection;

namespace Nox.Api.OData;

public static class ServiceExtensions
{
    public static IServiceCollection AddDynamicODataFeature(this IServiceCollection services)
    {
        services.AddControllers()
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .Count()
                .Expand()
                .SkipToken()
                .SetMaxTop(100)
        );

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApplicationModelProvider, EntityODataRoutingApplicationModelProvider>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<MatcherPolicy, EntityODataRoutingMatcherPolicy>());

        return services;
    }

    public static IServiceCollection AddDynamicQueriesAndCommands(this IServiceCollection services)
    {
        // TODO: move to a helper class and make configurable
        var assembly = Assembly.GetEntryAssembly();

        if (assembly != null)
        {
            var allTypes = assembly.ExportedTypes;

            // Get dynamically generated abstract base classes for queries/commands
            var baseTypes = allTypes
               .Where(t => t.IsAbstract && (typeof(IDynamicCommandHandler).IsAssignableFrom(t) || typeof(IDynamicQuery).IsAssignableFrom(t)));

            // Register DbContext
            var dbContextType = allTypes
                .FirstOrDefault(t => typeof(IDynamicNoxDomainDbContext).IsAssignableFrom(t));
            dbContextType?
                .GetMethod("RegisterContext")? // TODO: Move Method name into global generation constants
                .Invoke(null, new object[] { services });

            foreach (var type in baseTypes)
            {
                // Find the corresponding custom implementation
                var implementation = allTypes.FirstOrDefault(t => type.IsAssignableFrom(t) && !t.IsAbstract);
                if (implementation is not null)
                {
                    // Register the implementation for the base abstract query/command handler class
                    services.AddScoped(type, implementation);
                }
            }
        }

        return services;
    }
}