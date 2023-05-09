using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nox.Api.OData.Routing;

namespace Nox.Api.OData;

public static class ServiceExtensions
{
    public static IServiceCollection AddDynamicODataFeature(this IServiceCollection services)
    {
        services.AddControllers().AddOData(options => options
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
}