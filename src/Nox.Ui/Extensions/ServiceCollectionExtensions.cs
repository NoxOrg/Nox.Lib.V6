using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using MudBlazor.Services;
using Nox.Core.Extensions;
using Nox.Ui.Data;

namespace Nox;

public static class ServiceCollectionExtensions
{
    private const string DefaultBaseUrl = "https://localhost:5001/api/";
    public static IServiceCollection AddNoxUi(this IServiceCollection services)
    {

        services.AddAndConfigureNoxUiServices(c => c.BaseAddress = new Uri(DefaultBaseUrl)); ;

        return services; 

    }

    public static IServiceCollection AddNoxUi(this IServiceCollection services, Action<HttpClient> configureClient)
    {

        services.AddAndConfigureNoxUiServices(configureClient);

        return services;

    }

    private static IServiceCollection AddAndConfigureNoxUiServices(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        services.AddHttpClient("NoxUi", httpClient =>
        {
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json;odata.metadata=verbose");
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent, $"NoxUi-{AppDomain.CurrentDomain.FriendlyName}");

            configureClient?.Invoke(httpClient);
        });
    
        services.AddTransient<NoxDataService>();

        services.AddNoxConfiguration();

        services.AddMudServices();

        return services;
    }

}
