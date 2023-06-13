using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nox.Api;
using Nox.Api.OData.Constants;
using Nox.Api.OData.Swagger;
using Nox.Core.Interfaces;
using System.Reflection;
using Nox.Solution;

namespace Nox;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        builder.UseDomainQueriesAndCommands();

        builder.UseODataEntitySectionsSwaggerFilter();

        if (builder.ApplicationServices.GetService<NoxSolution>() == null)
        {
            return builder;
        }

        builder.UseMiddleware<DynamicApiMiddleware>().UseRouting();

        builder.UseHangfireDashboard("/jobs");

        return builder;
    }

    public static IApplicationBuilder UseODataEntitySectionsSwaggerFilter(
        this IApplicationBuilder appBuilder)
    {
        ODataEntitySectionsSwaggerFilter.Initialize(
            appBuilder.ApplicationServices.GetService<ILogger<ODataEntitySectionsSwaggerFilter>>(),
            appBuilder.ApplicationServices.GetService<IDynamicService>());

        return appBuilder;
    }

    /// <summary>
    /// Method find and executes the generated Request Mappings for Queries/Command handlers
    /// </summary>
    /// <param name="appBuilder">The app builder.</param>
    /// <returns>The app builder.</returns>
    public static IApplicationBuilder UseDomainQueriesAndCommands(
        this IApplicationBuilder appBuilder)
    {
        var dynamicService = appBuilder.ApplicationServices.GetService<IDynamicService>();

        if (dynamicService != null)
        {
            var entities = dynamicService.Entities;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null && entities != null)
            {
                // TODO: move ApiHelper to the global generation constants
                var apiHelper = assembly.ExportedTypes.FirstOrDefault(t => t.Name.Equals("ApiHelper"));
                var app = appBuilder as IEndpointRouteBuilder;

                if (app != null && apiHelper != null)
                {
                    foreach (var entity in entities)
                    {
                        // Get all queries and commands names
                        var queriesAndCommands = entity.Value
                            .Queries
                            .Select(q => q.Name)
                            .Union(entity.Value
                                .Commands
                                .Select(c => c.Name));

                        foreach (var name in queriesAndCommands)
                        {
                            // TODO: Move path generation to a helper method in order to avoid duplication
                            var path = $"/{RoutingConstants.ODATA_ROUTE_PREFIX}/{entity.Value.PluralName}/{name}";

                            // Invoke generated Map method
                            apiHelper.GetMethod(name)?
                                .Invoke(null, new object[] { app, path });
                        }
                    }
                }
            }
        }

        return appBuilder;
    }
}