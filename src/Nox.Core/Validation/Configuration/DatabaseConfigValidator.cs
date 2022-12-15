using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class DatabaseConfigValidator: AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigValidator()
    {
        RuleFor( db => db.Name)
            .NotEmpty()
            .WithMessage(db => string.Format(ValidationResources.DbNameEmpty, db.DefinitionFileName));
        
        RuleFor( db => db.Provider)
            .NotEmpty()
            .WithMessage(db => string.Format(ValidationResources.DbProviderEmpty, db.Name, db.DefinitionFileName));
    }
}