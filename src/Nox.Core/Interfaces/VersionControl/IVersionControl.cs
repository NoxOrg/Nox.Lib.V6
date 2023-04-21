namespace Nox.Core.Interfaces.VersionControl;

public interface IVersionControl: IMetaBase
{
    public string Provider { get; set; }
    public string Server { get; set; }
    public string Project { get; set; }
    public string Repository { get; set; }
    public string RelativeProjectSourceFolder { get; set; }
    public string RelativeDockerFilePath { get; set; }
}