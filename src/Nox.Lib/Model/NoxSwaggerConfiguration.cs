using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nox.Model
{
    public class NoxSwaggerConfiguration
    {
        public bool UseSwaggerGen { get; set; } = true;
        public bool UseODataEntitySectionsSwaggerFilter { get; set; } = true;
        public Action<SwaggerGenOptions> SetupAction { get; set; } = cfg =>
        {
            cfg.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "0.01",
                Title = "Nox Sample API. To change this description use 'swaggerConfiguration' parameter inside AddNox method."
            });
        };
    }
}
