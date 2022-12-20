using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.Postgres;

public static class ServiceExtensions
{
    public static IServiceCollection AddPostgresDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, PostgresDatabaseProvider>();
        return services;
    }
}