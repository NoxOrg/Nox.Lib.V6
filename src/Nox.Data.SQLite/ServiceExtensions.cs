using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.SQLite;

public static class ServiceExtensions
{
    public static IServiceCollection AddSqLiteDatabaseProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, SqLiteDatabaseProvider>();
        return services;
    }
}