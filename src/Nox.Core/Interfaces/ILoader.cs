namespace Nox.Core.Interfaces;

public interface ILoader: IMetaBase
{
    string Name { get; set; }
    string Description { get; set; }
    ILoaderSchedule? Schedule { get; set; }
    ILoaderLoadStrategy? LoadStrategy { get; set; }
    ILoaderTarget? Target { get; set; }
    ICollection<ILoaderSource>? Sources { get; set; }

    bool ApplyDefaults();
}