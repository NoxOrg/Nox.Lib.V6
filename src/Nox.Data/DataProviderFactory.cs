using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Data;

public class DataProviderFactory : IDataProviderFactory
{
    private readonly Func<IEnumerable<IDataProvider>> _factory;

    public DataProviderFactory(Func<IEnumerable<IDataProvider>> factory)
    {
        _factory = factory;
    }

    public IDataProvider Create(DataConnectionProvider provider)
    {
        var dbProviders = _factory!();
        //todo Check that name == provider
        var dbProvider = dbProviders.First(p => p.Name.Equals("", StringComparison.OrdinalIgnoreCase));
        return dbProvider;
    }
}