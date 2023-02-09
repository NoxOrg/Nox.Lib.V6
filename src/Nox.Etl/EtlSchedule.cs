using Nox.Core.Interfaces.Etl;
using Nox.Cron;

namespace Nox.Etl;

public class EtlSchedule: IEtlSchedule
{
    public string Start { get; set; }
    public string CronExpression { get; set; }
    
    IEtlRetryPolicy? IEtlSchedule.Retry
    {
        get => Retry;
        set => Retry = value as EtlRetryPolicy;
    }
        
    public EtlRetryPolicy? Retry { get; set; }
   
    public bool ApplyDefaults()
    {
        CronExpression = Start.ToCronExpression().ToString();

        return true;
    }
}