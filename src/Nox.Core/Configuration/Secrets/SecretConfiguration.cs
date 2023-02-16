namespace Nox.Core.Configuration.Secrets;

public class SecretConfiguration
{
    public string Provider { get; set; } = "azure-keyvault";

    public string Url { get; set; } = string.Empty;
    public SecretsValidForConfiguration? ValidFor { get; set; } = new SecretsValidForConfiguration { Minutes = 10 };
}