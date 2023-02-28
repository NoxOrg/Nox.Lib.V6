using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlSourceHttp: MetaBase, IEtlSourceHttp
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Verb { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    IEtlHttpAuth? IEtlSourceHttp.Auth
    {
        get => Auth;
        set => Auth = value as EtlHttpAuth;
    }
    
    public EtlHttpAuth? Auth { get; set; }
}