namespace Nox.Core.Interfaces.Etl;

public interface IEtlSchedule
{
    string Start { get; set; }
    string CronExpression { get; set; }
    IEtlRetryPolicy? Retry { get; set; }
    bool ApplyDefaults();
}