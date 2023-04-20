using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Nox.Core.Extensions;
using Nox.Ui.Data;

namespace Nox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoxUi(this IServiceCollection services)
    {

        services.AddHttpClient("NoxUi", httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://localhost:5001/api/");

            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json;odata=verbose");
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent, $"NoxUi-{AppDomain.CurrentDomain.FriendlyName}");
        });

        services.AddTransient<NoxDataService>();

        return services.AddNoxConfiguration();
    }
}
