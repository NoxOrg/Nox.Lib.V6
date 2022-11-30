namespace Nox.Core.Interfaces.Etl;

public interface ILoaderSchedule
{
    string Start { get; set; }
    string CronExpression { get; set; }
    ILoaderScheduleRetryPolicy? Retry { get; set; }
    bool RunOnStartup { get; set; }

    bool ApplyDefaults();
}