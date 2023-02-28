namespace Nox.Core.Interfaces.Dto;

public interface IDtoAttribute: IMetaBase
{
    string Name { get; set; }
    string Type { get; set; }

    bool ApplyDefaults();
    bool IsDateTimeType();
    Type NetDataType();
}