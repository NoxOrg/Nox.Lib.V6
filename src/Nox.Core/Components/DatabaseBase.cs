using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Interfaces;

namespace Nox.Core.Components;

public class DatabaseBase : MetaBase, IServiceDatabase
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = "localhost";
    public string User { get; set; } = "user";
    public int Port { get; set; } = 0;
    public string Password { get; set; } = "password";
    public string Options { get; set; } = "";
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
        
    [NotMapped]
    public IDatabaseProvider DatabaseProvider { get; set; }

    public virtual bool ApplyDefaults()
    {
        var isValid = true;

        Provider = Provider.Trim().ToLower();

        switch (Provider)
        {
            case "sqlserver":
                if (Port == 0) Port = 1433;
                break;

            case "postgres":
                if (Port == 0) Port = 5432;
                break;

            case "mysql":
                if (Port == 0) Port = 3306;
                break;

            default:
                isValid = false;
                break;
        }

        return isValid;
    }
}

