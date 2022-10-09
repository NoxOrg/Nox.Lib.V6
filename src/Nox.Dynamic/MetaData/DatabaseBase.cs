using FluentValidation;
using Nox.Dynamic.DatabaseProviders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Dynamic.MetaData;

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
    public IDatabaseProvider? DatabaseProvider { get; set; }

    internal virtual bool ApplyDefaults(ServiceValidationInfo info)
    {
        var isValid = true;

        if (string.IsNullOrEmpty(ConnectionString))
        {
            if (!string.IsNullOrEmpty(ConnectionVariable))
            {
                ConnectionString = info.ConfigurationValiables[ConnectionVariable];
            }
        }

        Provider = Provider.Trim().ToLower();

        switch (Provider)
        {
            case "sqlserver":
                if (Port == 0) Port = 1443;
                DatabaseProvider = new SqlServerDatabaseProvider(this, info.ServiceName);
                break;

            case "postgres":
                if (Port == 0) Port = 5432;
                DatabaseProvider = new PostgresDatabaseProvider(this, info.ServiceName);
                break;

            default:
                isValid = false;
                break;
        }

        return isValid;
    }
}

internal class DatabaseValidator : AbstractValidator<DatabaseBase>
{
    public DatabaseValidator(ServiceValidationInfo info)
    {

        RuleFor(db => db.ApplyDefaults(info))
            .NotEqual(false)
            .WithMessage(db => $"[{info.ServiceName}] Database provider '{db.Provider}' defined in {db.DefinitionFileName} is not supported");

    }
}