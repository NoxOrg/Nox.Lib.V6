namespace Nox.Core.Interfaces.Etl;

public interface IEtlSourceHttp: IMetaBase
{
    string Name { get; set; }
    string Format { get; set; }
    string Verb { get; set; }
    string Url { get; set; }
    IEtlHttpAuth? Auth { get; set; }
    IEtlSchedule? Schedule { get; set; }
}