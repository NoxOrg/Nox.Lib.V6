namespace Nox.Dynamic.MetaData
{
    public sealed class LoaderSchedule : MetaBase
    {
        public string Start { get; set; } = string.Empty;
        public LoaderScheduleRetryPolicy Retry { get; set; } = new();
    }
}
