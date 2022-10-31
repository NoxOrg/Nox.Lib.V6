using FluentValidation;
using Nox.Data;
using System.Collections.ObjectModel;

namespace Nox.Dynamic.MetaData;

public sealed class Loader : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LoaderSchedule Schedule { get; set; } = new();
    public LoaderLoadStrategy LoadStrategy { get; set; } = new();
    public LoaderTarget Target { get; set; } = new();
    public ICollection<LoaderSource> Sources { get; set; } = new Collection<LoaderSource>();

    public bool ApplyDefaults()
    {
        Schedule.ApplyDefaults();

        return true;
    }

}

internal class LoaderValidator : AbstractValidator<Loader>
{
    public LoaderValidator()
    {

        RuleFor(loader => loader.Name)
            .NotEmpty()
            .WithMessage(loader => $"The data loader name must be specified in {loader.DefinitionFileName}");

        RuleFor(loader => loader.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(loader => $"Defaults could not be applied to lader defined in {loader.DefinitionFileName}");

        RuleForEach(loader => loader.Sources)
            .SetValidator(new LoaderSourceValidator());

    }
}

