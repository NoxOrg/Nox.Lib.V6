using FluentValidation;
using Nox.Core.Models;
using Nox.Microservice.Validation;

namespace Nox.Microservice;

public static class ValidationExtensions
{
    public static void Validate(this MetaService service)
    {
        var validator = new MetaServiceValidator();

        validator.ValidateAndThrow(service);
    }
}