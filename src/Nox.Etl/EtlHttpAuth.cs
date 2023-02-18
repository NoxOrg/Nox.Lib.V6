using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlHttpAuth: IEtlHttpAuth
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}