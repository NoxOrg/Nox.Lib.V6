using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class LoaderScheduleConfiguration: MetaBase
{
    public string Start { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public LoaderScheduleRetryPolicyConfiguration? Retry { get; set; } = new();
    public bool? RunOnStartup { get; set; } = true;
}