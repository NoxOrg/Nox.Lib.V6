using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.SqlServer;

public static class ServiceExtensions
{
    public static IServiceCollection AddSqlServerDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, SqlServerDatabaseProvider>();
        return services;
    }
}