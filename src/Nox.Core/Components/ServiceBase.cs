// Minimal object to read required configurations from Nox service definition

using Nox.Core.Constants;

namespace Nox.Core.Components;

internal class ServiceBase
{
    public string KeyVaultUri { get; set; } = KeyVault.DefaultKeyVaultUri;

    public MessageBusBase MessageBus { get; set; } = new();
    public string EndpointProvider { get; set; } = string.Empty;
}

