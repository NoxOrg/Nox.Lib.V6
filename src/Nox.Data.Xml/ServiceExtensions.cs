using Microsoft.Extensions.DependencyInjection;
using Nox.Core.Interfaces.Database;

namespace Nox.Data.Xml;

public static class ServiceExtensions
{
    public static IServiceCollection AddXmlDataProvider(this IServiceCollection services)
    {
        services.AddTransient<IDataProvider, XmlDataProvider>();
        return services;
    }
}