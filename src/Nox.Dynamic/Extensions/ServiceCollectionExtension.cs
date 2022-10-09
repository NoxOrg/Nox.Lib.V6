using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nox.Dynamic.OData.Models;
using Nox.Dynamic.OData.Routing;

namespace Nox.Dynamic.Extensions
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddNox(this IServiceCollection services)
        {
            return services.AddDynamicOData();
        }

        public static IServiceCollection AddDynamicOData(this IServiceCollection services)
        {
            services.AddControllers().AddOData(options => options
                .Select().Filter().OrderBy().Count().Expand().SkipToken().SetMaxTop(100)
            );

            services.AddSingleton<DynamicModel>();

            services.AddDbContext<DynamicDbContext>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, EntityODataRoutingApplicationModelProvider>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, EntityODataRoutingMatcherPolicy>());

            return services;
        }
    }
}
