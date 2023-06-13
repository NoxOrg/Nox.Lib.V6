using Nox.Core.Interfaces.Messaging;

namespace Nox.Core.Interfaces.Etl;

public interface ILoader: IMetaBase
{
    string Name { get; set; }
    string Description { get; set; }
    ILoaderSchedule? Schedule { get; set; }
    ILoaderLoadStrategy? LoadStrategy { get; set; }
    ILoaderTarget? Target { get; set; }
    ICollection<IMessageTarget>? Messaging { get; set; }
    ICollection<ILoaderSource>? Sources { get; set; }

    bool ApplyDefaults();
}