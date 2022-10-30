namespace Nox.Configuration.Models;

internal class MessageBusBase
{
    public string Provider { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }
}

