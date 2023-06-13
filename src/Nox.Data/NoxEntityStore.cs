using Nox.Core.Interfaces.Database;

namespace Nox.Data;

public class NoxEntityStore: IServiceDataSource
{
    public int Id { get; set; }
    public string? ConnectionString { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DataConnectionProvider Provider { get; set; } = DataConnectionProvider.SqlServer;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string User { get; set; } = string.Empty;
    public IDataProvider? DataProvider { get; set; }
    public bool ApplyDefaults()
    {
        var isValid = true;

        switch (Provider)
        {
            case DataConnectionProvider.SqlServer:
                if (Port == 0) Port = 1433;
                break;

            case DataConnectionProvider.Postgres:
                if (Port == 0) Port = 5432;
                break;

            case DataConnectionProvider.MySql:
                if (Port == 0) Port = 3306;
                break;

            case DataConnectionProvider.SqLite:
                break;
            default:
                isValid = false;
                break;
        }

        return isValid;
    }
}