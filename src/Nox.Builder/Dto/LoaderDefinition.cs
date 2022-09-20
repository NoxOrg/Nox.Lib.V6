using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    internal class LoaderDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LoaderSchedule? Schedule { get; set; }
        public LoaderLoadStrategy? LoadStrategy { get; set; }
        public LoaderTarget? Target { get; set; }
        public List<LoaderSource> Sources { get; set; } = new();
    }

    internal class LoaderLoadStrategy
    {
        public string Type { get; set; } = string.Empty;
        public string[]? Columns { get; set; }
    }

    internal class LoaderTarget
    {
        public string Table { get; set; } = string.Empty;
        public string? Schema { get; set; } 
    }

    internal class LoaderSource
    {
        public string ConnectionVariable { get; set; } = string.Empty;
        public string DatabaseProvider { get; set; } = "SqlServer";
        public string Query { get; set; } = string.Empty;
        public int MinimumExpectedRecords { get; set; } = 0;
    }

    internal class LoaderSchedule
    {
        public string Start { get; set; } = string.Empty;
        public LoaderScheduleRetryPolicy? Retry { get; set; }
    }


    internal class LoaderScheduleRetryPolicy
    {
        public int Limit { get; set; } = 5;
        public int DelaySeconds { get; set; } = 60;
        public int DoubleDelayLimit { get; set; } = 10;
    }
}
