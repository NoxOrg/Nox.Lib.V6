using Nox.Core.Configuration;
using Nox.Core.Configuration.Secrets;

namespace Nox.Core.Interfaces.Configuration;

public interface IProjectConfiguration
{
    string Name { get; set; }
    string Description { get; set; }
    SecretConfiguration? Secrets { get; set; }
    string EndpointProvider { get; set; }
    public VersionControlConfiguration? VersionControl { get; set; }
    public TeamConfiguration? Team { get; set; }
    DataSourceConfiguration? Database { get; set; }
    List<MessagingProviderConfiguration>? MessagingProviders { get; set; }
    List<ApiConfiguration>? Apis { get; set; }
    List<EntityConfiguration>? Entities { get; set; }
    List<LoaderConfiguration>? Loaders { get; set; }
    List<DataSourceConfiguration>? DataSources { get; set; }
}