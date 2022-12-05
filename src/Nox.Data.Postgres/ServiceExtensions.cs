using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

namespace Nox.Data.Postgres;

public static class ServiceExtensions
{
    public static IServiceCollection AddPostgresDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseProvider, PostgresDatabaseProvider>();
        return services;
    }
}