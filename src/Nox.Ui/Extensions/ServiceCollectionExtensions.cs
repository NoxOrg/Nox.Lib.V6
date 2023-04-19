using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Extensions;
using Nox.Ui.Data;

namespace Nox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoxUi(this IServiceCollection services)
    {
        services.AddHttpClient<NoxDataService>( c => 
            c.BaseAddress = new Uri("https://localhost:5001/api")
        );

        services.AddTransient<NoxDataService>();

        return services.AddNoxConfiguration();
    }
}
