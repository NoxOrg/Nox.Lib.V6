using FluentValidation;
using Nox.Core.Interfaces;

namespace Nox.Core.Validation;

public class MetaServiceValidator : AbstractValidator<IProjectConfiguration>
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

        RuleForEach(service => service.MessagingProviders)
            .SetValidator(new ServiceMessageBusValidator());

    }
}