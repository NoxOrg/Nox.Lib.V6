using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;

namespace Nox.Data.SqlServer;

public static class ServiceExtensions
{
    public static IServiceCollection AddSqlServerDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDatabaseProvider, SqlServerDatabaseProvider>();
        return services;
    }
}