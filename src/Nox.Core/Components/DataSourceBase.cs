using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Database;

namespace Nox.Core.Components;

public class DataSourceBase : MetaBase, IServiceDataSource
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = "localhost";
    public string User { get; set; } = "user";
    public int Port { get; set; } = 0;
    public string Password { get; set; } = "password";
    public string Options { get; set; } = "";
    [MaxLength(512)]
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
        
    [NotMapped]
    public IDataProvider? DataProvider { get; set; }

    public virtual bool ApplyDefaults()
    {
        var isValid = true;

        Provider = Provider.Trim().ToLower();

        switch (Provider)
        {
            case Constants.DataProvider.SqlServer:
                if (Port == 0) Port = 1433;
                break;

            case Constants.DataProvider.Postgres:
                if (Port == 0) Port = 5432;
                break;

            case Constants.DataProvider.MySql:
                if (Port == 0) Port = 3306;
                break;

            case Constants.DataProvider.Json:
                break;
            case Constants.DataProvider.SqLite:
                break;
            default:
                isValid = false;
                break;
        }

        return isValid;
    }
}

