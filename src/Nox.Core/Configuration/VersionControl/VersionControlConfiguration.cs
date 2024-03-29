﻿
using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class VersionControlConfiguration : MetaBase
{
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string RelativeProjectSourceFolder { get; set; } = string.Empty;
    public string RelativeDockerFilePath { get; set; } = string.Empty;
}

