using Nox.Core.Components;

namespace Nox.Core.Configuration.Secrets;

public class SecretConfiguration: MetaBase
{
    public SecretsValidForConfiguration? ValidFor { get; set; } = new SecretsValidForConfiguration { Minutes = 10 };
    public IList<SecretProviderConfiguration>? Providers { get; set; }
}