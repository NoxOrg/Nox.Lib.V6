using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Nox.Core.Extensions;

namespace Nox
{
    public class NoxSwaggerConfiguration
    {
        public bool UseSwaggerGen { get; set; } = true;
        public bool UseODataEntitySectionsSwaggerFilter { get; set; } = true;
        public Action<SwaggerGenOptions> SetupAction { get; set; } = cfg =>
        {
            var projectConfig = ServiceCollectionExtensions.NoxProjectConfiguration;

            cfg.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "0.01",
                Title = projectConfig?.Name,
                Description = projectConfig?.Description
            });
        };
    }
}
