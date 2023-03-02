using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces;

namespace Nox.Api.OData.Swagger
{
    public static class WebApplicationCollectionExtensions
    {
        public static WebApplication UseODataEntitySectionsSwaggerFilter(
            this WebApplication appBuilder)
        {
            ODataEntitySectionsSwaggerFilter.Initialize(
                appBuilder.Services.GetService<ILogger<ODataEntitySectionsSwaggerFilter>>(),
                appBuilder.Services.GetService<IDynamicService>());

            return appBuilder;
        }
    }
}
