using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces;
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

        services.AddSingleton<Func<IEnumerable<IDatabaseProvider>>>(x => 
            () => x.GetService<IEnumerable<IDatabaseProvider>>()!
        );

        services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();

        return services;
    }
    
    public static IServiceCollection AddDynamicModel(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicModel, DynamicModel>();
        return services;
    }

    public static IServiceCollection AddDynamicDbContext(this IServiceCollection services)
    {
        services.AddDbContext<IDynamicDbContext, DynamicDbContext>();

        return services;
    }
}
