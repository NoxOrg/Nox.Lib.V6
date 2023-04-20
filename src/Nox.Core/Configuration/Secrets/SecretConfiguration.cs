namespace Nox.Core.Configuration.Secrets;

public class SecretConfiguration 
{
    public SecretsValidForConfiguration? ValidFor { get; set; } = new SecretsValidForConfiguration { Minutes = 10 };
    public IList<SecretProviderConfiguration>? Providers { get; set; }
}