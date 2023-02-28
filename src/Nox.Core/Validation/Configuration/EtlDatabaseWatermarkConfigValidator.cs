using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EtlDatabaseWatermarkConfigValidator: AbstractValidator<EtlDatabaseWatermarkConfiguration>
{
    public EtlDatabaseWatermarkConfigValidator()
    {
        RuleFor(p => p.SequentialKeyColumn)
            .NotEmpty()
            .Unless(p => p.DateColumns.Length > 0)
            .WithMessage(entity => string.Format(ValidationResources.EntityNameEmpty, entity.DefinitionFileName));
        RuleFor(p => p.DateColumns).NotEmpty().Unless(p => !string.IsNullOrEmpty(p.SequentialKeyColumn));
    }
}