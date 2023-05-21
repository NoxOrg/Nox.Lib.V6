using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class EntityQueryConfigValidator : AbstractValidator<QueryConfiguration>
{
    public EntityQueryConfigValidator()
    {
        RuleFor(query => query.Name)
            .NotEmpty()
            .WithMessage(query => string.Format(ValidationResources.EntityQueryNameEmpty, query.DefinitionFileName));

        RuleFor(query => query.Response.ResponseDto)
            .NotEmpty()
            .WithMessage(query => string.Format(ValidationResources.EntityQueryResponseEmpty, query.Name, query.DefinitionFileName));

        RuleForEach(entity => entity.Parameters)
            .SetValidator(new EntityQueryParameterConfigValidator());
    }
}