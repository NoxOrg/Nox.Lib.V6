using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Secrets;

namespace Nox.Core.Interfaces;

public interface IProjectConfiguration 
{
    string Name { get; set; }
    string Description { get; set; }
    string KeyVaultUri { get; set; }
    bool AutoMigrations { get; set; }

    ISecret? Secrets { get; set; }

    IServiceDataSource? Database { get; set; }
    ICollection<IMessagingProvider>? MessagingProviders { get; set; }
    ICollection<IServiceDataSource>? DataSources { get; set; }
    ICollection<IEntity>? Entities { get; set; }
    ICollection<ILoader>? Loaders { get; set; }
    ICollection<IApi>? Apis { get; set; }

    void Validate();
    void Configure();
}