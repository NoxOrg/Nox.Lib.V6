using Nox.Core.Components;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public class EtlHttp: MetaBase, IEtlHttp
{
    public string Name { get; set; }
    public string Format { get; set; }
    public string Verb { get; set; }
    public string Url { get; set; }
    
    IEtlHttpAuth? IEtlHttp.Auth
    {
        get => Auth;
        set => Auth = value as EtlHttpAuth;
    }
    
    public EtlHttpAuth? Auth { get; set; }
}