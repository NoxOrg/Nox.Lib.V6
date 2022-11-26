using System.Data;
using FluentValidation;
using Nox.Core.Models;
using Nox.Messaging;

namespace Nox.Microservice.Validation;

public class MessageBusValidator : AbstractValidator<MessageBusBase>
{
    protected MessageBusValidator()
    {
        RuleFor(mb => mb.Name)
            .NotEmpty()
            .WithMessage(mb => $"The Message Bus's name must be specified in {mb.DefinitionFileName}");

    }
}