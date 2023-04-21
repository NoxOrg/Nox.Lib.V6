using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Nox.Core.Extensions;
using Nox.Ui.Data;
using System.Runtime.CompilerServices;

namespace Nox;

public static class ServiceCollectionExtensions
{
    private const string DefaultBaseUrl = "https://localhost:5001/api/";
    public static IServiceCollection AddNoxUi(this IServiceCollection services)
    {

        services.AddAndConfigureNoxUiServices();

        return services; 

    }

    public static IServiceCollection AddNoxUi(this IServiceCollection services, Action<HttpClient> configureClient)
    {

        services.AddAndConfigureNoxUiServices(configureClient);

        return services;

    }

    private static IServiceCollection AddAndConfigureNoxUiServices(this IServiceCollection services, Action<HttpClient> configureClient = null!)
    {
        services.AddHttpClient("NoxUi", httpClient =>
        {
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json;odata=verbose");
            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent, $"NoxUi-{AppDomain.CurrentDomain.FriendlyName}");

            configureClient?.Invoke(httpClient);
        });
    
        services.AddTransient<NoxDataService>();

        services.AddNoxConfiguration();

        return services;
    }

}
