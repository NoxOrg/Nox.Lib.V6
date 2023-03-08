using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nox.Api;
using Nox.Api.OData.Swagger;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;

namespace Nox;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseNox(
        this IApplicationBuilder builder)
    {
        builder.UseODataEntitySectionsSwaggerFilter();

        if (builder.ApplicationServices.GetService<IProjectConfiguration>() == null) return builder;
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
}