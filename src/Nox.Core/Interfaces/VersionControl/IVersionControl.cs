namespace Nox.Core.Interfaces.VersionControl;

public interface IVersionControl: IMetaBase
{
    string Provider { get; set; }
    string Server { get; set; }
    string Project { get; set; }
    string Repository { get; set; }
    string RelativeProjectSourceFolder { get; set; }
    string RelativeDockerFilePath { get; set; }
}