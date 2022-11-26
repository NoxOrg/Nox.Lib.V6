using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

namespace Nox.Data.MySql;

public static class ServiceExtensions
{
    public static IServiceCollection AddMySqlDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseProvider, MySqlDatabaseProvider>();
        return services;
    }
}