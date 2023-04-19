using System.Collections.ObjectModel;
using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Core.Models;

public sealed class Loader : MetaBase, ILoader
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    ILoaderSchedule? ILoader.Schedule
    {
        get => Schedule;
        set => Schedule = value as LoaderSchedule;
    }

    public LoaderSchedule? Schedule { get; set; }

    ILoaderLoadStrategy? ILoader.LoadStrategy
    {
        get => LoadStrategy;
        set => LoadStrategy = value as LoaderLoadStrategy;
    }
    
    public LoaderLoadStrategy? LoadStrategy { get; set; }

    ILoaderTarget? ILoader.Target
    {
        get => Target;
        set => Target = value as LoaderTarget;
    }
    public LoaderTarget? Target { get; set; }

    ICollection<IMessageTarget>? ILoader.Messaging
    {
        get => Messaging?.ToList<IMessageTarget>();
        set => Messaging = value as ICollection<MessageTarget>;
    }

    public ICollection<MessageTarget>? Messaging { get; set; }

    ICollection<ILoaderSource>? ILoader.Sources
    {
        get => Sources?.ToList<ILoaderSource>();
        set => Sources = value as ICollection<LoaderSource>;
    }

    public ICollection<LoaderSource>? Sources { get; set; } = new Collection<LoaderSource>();

    public bool ApplyDefaults()
    {
        Schedule!.ApplyDefaults();
        return true;
    }

}



