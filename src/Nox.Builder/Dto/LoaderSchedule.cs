namespace Nox.Dynamic.Dto
{
    internal class LoaderSchedule
    {
        public string Start { get; set; } = string.Empty;
        public LoaderScheduleRetryPolicy Retry { get; set; } = new();
    }
}
