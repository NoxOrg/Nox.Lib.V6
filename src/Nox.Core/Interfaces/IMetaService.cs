using Nox.Core.Components;
using Nox.Core.Models;

namespace Nox.Core.Interfaces;

public interface IMetaService
{
    string Name { get; set; }
    string Description { get; set; }
    string KeyVaultUri { get; set; }
    IServiceDatabase Database { get; set; }
    IServiceMessageBus MessageBus { get; set; }
    ICollection<IEntity> Entities { get; set; }
    ICollection<Loader> Loaders { get; set; }
    ICollection<Api> Apis { get; set; }

    void Validate();
}