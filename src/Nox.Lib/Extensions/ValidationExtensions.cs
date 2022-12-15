using FluentValidation;
using Nox.Core.Validation;
using Nox.Lib;

namespace Nox;

public static class ValidationExtensions
{
    public static void Validate(this MetaService service)
    {
        var validator = new MetaServiceValidator();
        validator.ValidateAndThrow(service);
    }
}