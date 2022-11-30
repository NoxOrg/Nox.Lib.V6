using FluentValidation;
using Nox.Core.Components;

namespace Nox.Microservice.Validation;

public class MessageBusValidator : AbstractValidator<ServiceMessageBusBase>
{
    protected MessageBusValidator()
    {
        RuleFor(mb => mb.Name)
            .NotEmpty()
            .WithMessage(mb => $"The Message Bus's name must be specified in {mb.DefinitionFileName}");

    }
}