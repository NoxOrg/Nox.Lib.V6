using FluentValidation;
using Nox.Core.Configuration;

namespace Nox.Core.Validation.Configuration;

public class LoaderLoadStrategyConfigValidator: AbstractValidator<LoaderLoadStrategyConfiguration?>
{
    public LoaderLoadStrategyConfigValidator()
    {
        RuleFor(lls => lls!.Type)
            .NotEmpty()
            .WithMessage(lls => string.Format(ValidationResources.LoadStrategyTypeEmpty, lls!.DefinitionFileName));
        
        var typeConditions = new List<string>() { "dropandload", "mergenew" };
        RuleFor(lls => lls!.Type.ToLower())
            .Must(x => typeConditions.Contains(x.ToLower()))
            .WithMessage(lls => string.Format(ValidationResources.LoadStrategyTypeInvalid, "DropAndLoad/MergeNew", lls!.DefinitionFileName));
    }
}