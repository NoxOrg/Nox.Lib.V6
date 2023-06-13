using Nox.Core.Components;
using Nox.Solution;

namespace Nox.Data.Extensions;

public static class DataConnectionExtension
{
    public static DataSourceBase ToDataSource(this DataConnection dataConnection)
    {
        var result = new DataSourceBase
        {
            Name = dataConnection.Name,
            Options = dataConnection.Options!,
            Password = dataConnection.Password!,
            Port = dataConnection.Port!.Value,
            Server = dataConnection.ServerUri,
            User = dataConnection.User!,
            Provider = dataConnection.Provider!.Value
        };
        result.ApplyDefaults();
        return result;
    }

}