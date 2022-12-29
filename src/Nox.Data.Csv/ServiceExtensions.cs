using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.Csv;

public static class ServiceExtensions
{
    public static IServiceCollection AddJsonDataProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, CsvDataProvider>();
        return services;
    }
}