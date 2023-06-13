namespace Nox.Core.Interfaces.Etl;

public interface IEtlMap
{
    string SourceColumn { get; set; }
    string TargetAttribute { get; set; }
    string Converter { get; set; }
}