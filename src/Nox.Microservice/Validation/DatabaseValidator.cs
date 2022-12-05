using FluentValidation;
using Nox.Core.Interfaces;

namespace Nox.Microservice.Validation;

public class DatabaseValidator : AbstractValidator<IServiceDatabase>
{
    public DatabaseValidator()
    {
        RuleFor(db => db.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(db => $"Database provider '{db.Provider}' defined in {db.DefinitionFileName} is not supported");
    }
}