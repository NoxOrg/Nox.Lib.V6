namespace Nox.Core.Components;

internal class MessageBusProviderBase
{
    public string Provider { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}

