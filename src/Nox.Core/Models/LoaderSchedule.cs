using Nox.Core.Components;
using Nox.Cron;

namespace Nox.Core.Models
{
    public sealed class LoaderSchedule : MetaBase
    {
        public string Start { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public LoaderScheduleRetryPolicy? Retry { get; set; }
        public bool RunOnStartup { get; set; } = true; 

        public bool ApplyDefaults()
        {
            CronExpression = Start.ToCronExpression().ToString();

            return true;
        }
    }
}
