using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Data;

public class DatabaseProviderFactory : IDataProviderFactory
{
    private readonly Func<IEnumerable<IDataProvider>> _factory;

    public DatabaseProviderFactory(Func<IEnumerable<IDataProvider>> factory)
    {
        _factory = factory;
    }

    public IDataProvider Create(string provider)
    {
        var dbProviders = _factory!();
        var dbProvider = dbProviders.First(p => p.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
        return dbProvider;
    }
}