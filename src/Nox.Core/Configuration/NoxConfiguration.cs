using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;

namespace Nox.Core.Configuration;

public class NoxConfiguration: MetaBase, INoxConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = KeyVault.DefaultKeyVaultUri;
    public string EndpointProvider { get; set; } = string.Empty;
    public DatabaseConfiguration? Database { get; set; }
    public List<MessagingProviderConfiguration>? MessagingProviders { get; set; }
    public List<ApiConfiguration>? Apis { get; set; }
    public List<EntityConfiguration>? Entities { get; set; }
    public List<LoaderConfiguration>? Loaders { get; set; }
    public List<DatabaseConfiguration>? DataSources { get; set; }
}

