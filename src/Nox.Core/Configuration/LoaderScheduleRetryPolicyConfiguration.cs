namespace Nox.Core.Configuration;

public class LoaderScheduleRetryPolicyConfiguration
{
    public int Limit { get; set; } = 0;
    public int DelaySeconds { get; set; } = 0;
    public int DoubleDelayLimit { get; set; } = 0;
}