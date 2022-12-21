using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Core.Interfaces;

public interface IMetaService
{
    string Name { get; set; }
    string Description { get; set; }
    string KeyVaultUri { get; set; }
    bool AutoMigrations { get; set; }
    IServiceDataSource? Database { get; set; }
    ICollection<IMessagingProvider>? MessagingProviders { get; set; }
    ICollection<IServiceDataSource>? DataSources { get; set; }
    ICollection<IEntity>? Entities { get; set; }
    ICollection<ILoader>? Loaders { get; set; }
    ICollection<IApi>? Apis { get; set; }

    void Validate();
    void Configure();
}