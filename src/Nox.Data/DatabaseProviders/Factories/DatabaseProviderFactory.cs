using Microsoft.Extensions.DependencyInjection;

namespace Nox.Data
{
    public static class DatabaseProviderFactoryExtensions
    {
        public static IServiceCollection AddDatabaseProviderFactory(this IServiceCollection services)
        {
            services.AddTransient<IDatabaseProvider, SqlServerDatabaseProvider>();
            services.AddTransient<IDatabaseProvider, PostgresDatabaseProvider>();
            services.AddSingleton<Func<IEnumerable<IDatabaseProvider>>>(x => 
                () => x.GetService<IEnumerable<IDatabaseProvider>>()!
            );
            services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();

            return services;
        }
    }

    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly Func<IEnumerable<IDatabaseProvider>> _factory;

        public DatabaseProviderFactory(Func<IEnumerable<IDatabaseProvider>> factory)
        {
            _factory = factory;
        }

        public IDatabaseProvider Create(string provider)
        {
            var dbProviders = _factory!();
            var dbProvider = dbProviders.First(p => p.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
            return dbProvider;
        }
    }
}
