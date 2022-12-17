using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.JsonFile;

public static class ServiceExtensions
{
    public static IServiceCollection AddJsonFileDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, JsonDataProvider>();
        return services;
    }
}