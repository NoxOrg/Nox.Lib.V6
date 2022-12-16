using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data.MySql;
using Nox.Data.Postgres;
using Nox.Data.SqlServer;

namespace Nox.Data;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabaseProviderFactory(this IServiceCollection services)
    {
        services
            .AddSqlServerDatabaseProvider()
            .AddPostgresDatabaseProvider()
            .AddMySqlDatabaseProvider();

        services.AddSingleton<Func<IEnumerable<IDataProvider>>>(x => 
            () => x.GetService<IEnumerable<IDataProvider>>()!
        );

        services.AddSingleton<IDataProviderFactory, DatabaseProviderFactory>();

        return services;
    }

    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicModel, DynamicModel>();
        services.AddDbContext<IDynamicDbContext, DynamicDbContext>();
        return services;
    }
}
