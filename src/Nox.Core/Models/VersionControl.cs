using Nox.Core.Components;
using Nox.Core.Interfaces.VersionControl;

namespace Nox.Core.Models;

public sealed class VersionControl : MetaBase, IVersionControl
{
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string RelativeProjectSourceFolder { get; set; } = string.Empty;
    public string RelativeDockerFilePath { get; set; } = string.Empty;
}