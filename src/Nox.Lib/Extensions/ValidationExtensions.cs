using FluentValidation;
using Nox.Core.Validation;
using Nox.Core.Models;

namespace Nox;

public static class ValidationExtensions
{
    public static void Validate(this ProjectConfiguration service)
    {
        var validator = new MetaServiceValidator();
        validator.ValidateAndThrow(service);
    }
}