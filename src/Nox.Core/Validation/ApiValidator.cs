using FluentValidation;
using Nox.Core.Interfaces.Api;

namespace Nox.Core.Validation;

public class ApiValidator : AbstractValidator<IApi>
{
    public ApiValidator()
    {
        RuleFor( api => api.Name)
            .NotEmpty()
            .WithMessage(api => $"The api's name must be specified in {api.DefinitionFileName}");
    }
}