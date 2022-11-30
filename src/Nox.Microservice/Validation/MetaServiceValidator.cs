using FluentValidation;
using Nox.Core.Models;

namespace Nox.Microservice.Validation;

public class MetaServiceValidator : AbstractValidator<MetaService>
{
    public MetaServiceValidator()
    {
        RuleForEach(service => service.Entities)
            .SetValidator(new EntityValidator());

        RuleForEach(service => service.Loaders)
            .SetValidator(new LoaderValidator());

        RuleForEach(service => service.Apis)
            .SetValidator(new ApiValidator());

        RuleFor(service => service.Database)
            .SetValidator(new ServiceDatabaseValidator()!);

    }
}