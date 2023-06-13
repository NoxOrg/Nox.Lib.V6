namespace Nox.Core.Models;

public class MergeState
{
    public string Integration { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;

    public DateTime LastDateLoadedUtc { get; set; }

    public bool Updated { get; set; }

}