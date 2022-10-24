using Nox.Cron;

namespace Nox.Dynamic.MetaData
{
    public sealed class LoaderSchedule : MetaBase
    {
        public string Start { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public LoaderScheduleRetryPolicy Retry { get; set; } = new();

        public bool ApplyDefaults()
        {
            CronExpression = Start.ToCronExpression().ToString();

            return true;
        }
    }
}
