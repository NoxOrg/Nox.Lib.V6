using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl
{
    public sealed class LoaderScheduleRetryPolicy : MetaBase, ILoaderScheduleRetryPolicy
    {
        public int Limit { get; set; } = 5;
        public int DelaySeconds { get; set; } = 60;
        public int DoubleDelayLimit { get; set; } = 10;
    }
}
