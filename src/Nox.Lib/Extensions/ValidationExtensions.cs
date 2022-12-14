using FluentValidation;
using Nox.Core.Validation;

namespace Nox.Lib;

public static class ValidationExtensions
{
    public static void Validate(this MetaService service)
    {
        var validator = new MetaServiceValidator();
        validator.ValidateAndThrow(service);
    }
}