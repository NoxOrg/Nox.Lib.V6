using Nox.Core.Interfaces.Database;

namespace Nox.Data;

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