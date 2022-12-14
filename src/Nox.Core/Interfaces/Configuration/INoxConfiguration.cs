using Nox.Core.Configuration;

namespace Nox.Core.Interfaces.Configuration;

public interface INoxConfiguration
{
    string Name { get; set; }
    string Description { get; set; }
    string KeyVaultUri { get; set; }
    string EndpointProvider { get; set; }
    DatabaseConfiguration? Database { get; set; }
    List<MessagingProviderConfiguration>? MessagingProviders { get; set; }
    List<ApiConfiguration>? Apis { get; set; }
    List<EntityConfiguration>? Entities { get; set; }
    List<LoaderConfiguration>? Loaders { get; set; }
    List<DatabaseConfiguration>? DataSources { get; set; }
}