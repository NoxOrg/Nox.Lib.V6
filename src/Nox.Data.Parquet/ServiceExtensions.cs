using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.Parquet;

public static class ServiceExtensions
{
    public static IServiceCollection AddParquetDataProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, ParquetDataProvider>();
        return services;
    }
}