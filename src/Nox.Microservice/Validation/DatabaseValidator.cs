using FluentValidation;
using Nox.Core.Models;

namespace Nox.Microservice.Validation;

public class DatabaseValidator : AbstractValidator<DatabaseBase>
{
    public DatabaseValidator()
    {

        RuleFor(db => db.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(db => $"Database provider '{db.Provider}' defined in {db.DefinitionFileName} is not supported");

    }
}