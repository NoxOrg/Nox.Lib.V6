using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class LoaderConfiguration: MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LoaderScheduleConfiguration? Schedule { get; set; } = new();
    public LoaderLoadStrategyConfiguration? LoadStrategy { get; set; } = new();
    public LoaderTargetConfiguration? Target { get; set; } = new();
    public List<MessageTargetConfiguration>? Messaging { get; set; } = new();
    public List<LoaderSourceConfiguration>? Sources { get; set; } = new();
}