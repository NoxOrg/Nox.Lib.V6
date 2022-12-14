using FluentValidation;
using Nox.Core.Components;

namespace Nox.Core.Validation;

public class MessageBusValidator : AbstractValidator<MessagingProviderBase>
{
    protected MessageBusValidator()
    {
        RuleFor(mb => mb.Name)
            .NotEmpty()
            .WithMessage(mb => $"The Message Bus's name must be specified in {mb.DefinitionFileName}");

    }
}