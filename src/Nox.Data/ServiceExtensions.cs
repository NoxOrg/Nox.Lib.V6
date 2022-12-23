using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;
using Nox.Data.JsonFile;
using Nox.Data.MySql;
using Nox.Data.Postgres;
using Nox.Data.SQLite;
using Nox.Data.SqlServer;

namespace Nox.Data;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataProviderFactory(this IServiceCollection services)
    {
        services
            .AddSqlServerDatabaseProvider()
            .AddPostgresDatabaseProvider()
            .AddMySqlDatabaseProvider()
            .AddJsonDataProvider()
            .AddSqLiteDatabaseProvider();

        services.AddSingleton<Func<IEnumerable<IDataProvider>>>(x => 
            () => x.GetService<IEnumerable<IDataProvider>>()!
        );

        services.AddSingleton<IDataProviderFactory, DataProviderFactory>();

        return services;
    }

    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicModel, DynamicModel>();
        services.AddDbContext<IDynamicDbContext, DynamicDbContext>();
        return services;
    }
}
