using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlHttpAuth: IEtlHttpAuth
{
    public string Name { get; set; }
    public string Type { get; set; }
}