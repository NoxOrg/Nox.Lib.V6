using FluentValidation;
using Nox.Core.Interfaces.Database;

namespace Nox.Core.Validation;

public class DatabaseValidator : AbstractValidator<IServiceDataSource>
{
    public DatabaseValidator()
    {
        RuleFor(db => db.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(db => $"Database provider '{db.Provider}' defined in {db.DefinitionFileName} is not supported");
    }
}