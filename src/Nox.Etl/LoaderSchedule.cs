using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;
using Nox.Cron;

namespace Nox.Etl
{
    public sealed class LoaderSchedule : MetaBase, ILoaderSchedule
    {
        public string Start { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;

        ILoaderScheduleRetryPolicy? ILoaderSchedule.Retry
        {
            get => Retry;
            set => Retry = value as LoaderScheduleRetryPolicy;
        }
        
        public LoaderScheduleRetryPolicy? Retry { get; set; }
        public bool RunOnStartup { get; set; } = true; 

        public bool ApplyDefaults()
        {
            CronExpression = Start.ToCronExpression().ToString();

            return true;
        }
    }
}
