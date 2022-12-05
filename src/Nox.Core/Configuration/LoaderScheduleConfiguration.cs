namespace Nox.Core.Configuration;

public class LoaderScheduleConfiguration
{
    public string Start { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public LoaderScheduleRetryPolicyConfiguration? Retry { get; set; } = new();
}