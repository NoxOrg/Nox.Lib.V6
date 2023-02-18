namespace Nox.Core.Configuration.Secrets;

public class SecretProviderConfiguration
{
    public string Provider { get; set; } = "azure-keyvault";

    public string Url { get; set; } = string.Empty;
}