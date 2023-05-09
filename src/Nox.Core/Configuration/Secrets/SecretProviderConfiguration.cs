using Nox.Core.Components;

namespace Nox.Core.Configuration.Secrets;

public class SecretProviderConfiguration: MetaBase
{
    public string Provider { get; set; } = "azure-keyvault";

    public string Url { get; set; } = string.Empty;
}