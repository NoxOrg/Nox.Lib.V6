// Minimal object to read required configurations from Nox service definition

namespace Nox.Configuration.Models;

internal class ServiceBase
{
    public string KeyVaultUri { get; set; } = KeyVault.DefaultKeyVaultUri;

    public MessageBusBase MessageBus { get; set; } = new();

}

