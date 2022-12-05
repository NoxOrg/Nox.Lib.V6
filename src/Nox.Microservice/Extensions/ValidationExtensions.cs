using FluentValidation;
using Nox.Microservice.Validation;

namespace Nox.Microservice.Extensions;

public static class ValidationExtensions
{
    public static void Validate(this MetaService service)
    {
        var validator = new MetaServiceValidator();
        validator.ValidateAndThrow(service);
    }
}