using FluentValidation;
using Nox.Core.Components;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Core.Validation;

public class MessageBusValidator : AbstractValidator<IMessagingProvider>
{
    public MessageBusValidator()
    {
        RuleFor(mb => mb.Name)
            .NotEmpty()
            .WithMessage(mb => $"The Message Bus's name must be specified in {mb.DefinitionFileName}");

        RuleFor(mb => mb.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(db => $"Messaging provider '{db.Provider}' defined in {db.DefinitionFileName} is not supported");

    }
}