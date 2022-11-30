﻿using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Etl
{
    public sealed class LoaderScheduleRetryPolicy : MetaBase, ILoaderScheduleRetryPolicy
    {
        public int Limit { get; set; } = 5;
        public int DelaySeconds { get; set; } = 60;
        public int DoubleDelayLimit { get; set; } = 10;
    }
}