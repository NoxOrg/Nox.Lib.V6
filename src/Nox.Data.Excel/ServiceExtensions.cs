using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.Excel;

public static class ServiceExtensions
{
    public static IServiceCollection AddExcelDataProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, ExcelDataProvider>();
        return services;
    }
}