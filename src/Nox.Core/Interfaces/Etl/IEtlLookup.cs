namespace Nox.Core.Interfaces.Etl;

public interface IEtlLookup
{
    string SourceColumn { get; set; }
    string DataSource { get; set; }
    string MatchColumn { get; set; }
    string ReturnColumn { get; set; }
    string TargetAttribute { get; set; }
}