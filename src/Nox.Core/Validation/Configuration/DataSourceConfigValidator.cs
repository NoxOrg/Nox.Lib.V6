using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class DataSourceConfigValidator: AbstractValidator<DataSourceConfiguration>
{
    public DataSourceConfigValidator()
    {
        RuleFor( db => db.Name)
            .NotEmpty()
            .WithMessage(db => string.Format(ValidationResources.DbNameEmpty, db.DefinitionFileName));
        
        RuleFor( db => db.Provider)
            .NotEmpty()
            .WithMessage(db => string.Format(ValidationResources.DbProviderEmpty, db.Name, db.DefinitionFileName));
        
        var providerConditions = new List<string>() { "sqlserver", "postgres", "mysql", "json", "sqlite", "csv", "excel", "parquet", "xml" };
        RuleFor(dsc => dsc!.Provider.ToLower())
            .Must(x => providerConditions.Contains(x.ToLower()))
            .WithMessage(dsc => string.Format(ValidationResources.DbProviderInvalid, "SqlServer/Postgres/MySql/Sqlite/Json/Csv/Excel/Parquet/Xml", dsc!.DefinitionFileName));
    }
}