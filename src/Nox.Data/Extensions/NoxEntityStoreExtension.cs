using Nox.Core.Exceptions;
using Nox.Solution;

namespace Nox.Data.Extensions;

public static class NoxEntityStoreExtension
{
    public static NoxEntityStore ToEntityStore(this DatabaseServer databaseServer)
    {
        var result = new NoxEntityStore
        {
            Name = databaseServer.Name,
            Options = databaseServer.Options!,
            Password = databaseServer.Password!,
            Port = databaseServer.Port!.Value,
            Server = databaseServer.ServerUri,
            User = databaseServer.User!,
            Provider = GetProvider(databaseServer.Provider),
        };
        result.ApplyDefaults();
        return result;
    }

    private static DataConnectionProvider GetProvider(DatabaseServerProvider serverProvider)
    {
        switch (serverProvider)
        {
            case DatabaseServerProvider.SqlServer:
                return DataConnectionProvider.SqlServer;
            case DatabaseServerProvider.Postgres:
                return DataConnectionProvider.Postgres;
            case DatabaseServerProvider.MySql:
                return DataConnectionProvider.MySql;
            case DatabaseServerProvider.SqLite:
                return DataConnectionProvider.SqLite;
        }

        throw new NoxException("Unable to determine NoxEntityStore data provider!");
        
    }
}