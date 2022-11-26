using System.Collections.ObjectModel;
using FluentValidation;
using Nox.Core.Components;

namespace Nox.Core.Models;

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



