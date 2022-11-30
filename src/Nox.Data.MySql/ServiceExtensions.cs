using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.MySql;

public static class ServiceExtensions
{
    public static IServiceCollection AddMySqlDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseProvider, MySqlDatabaseProvider>();
        return services;
    }
}