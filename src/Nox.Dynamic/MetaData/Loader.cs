using FluentValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData;

public sealed class Loader : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LoaderSchedule Schedule { get; set; } = new();
    public LoaderLoadStrategy LoadStrategy { get; set; } = new();
    public LoaderTarget Target { get; set; } = new();
    public ICollection<LoaderSource> Sources { get; set; } = new Collection<LoaderSource>();

}

internal class LoaderValidator : AbstractValidator<Loader>
{
    public LoaderValidator(ServiceValidationInfo info)
    {

        RuleFor(loader => loader.Name)
            .NotEmpty()
            .WithMessage(loader => $"[{info.ServiceName}] The data loader name must be specified in {loader.DefinitionFileName}");

        RuleForEach(loader => loader.Sources)
            .SetValidator(new LoaderSourceValidator(info));

    }
}

