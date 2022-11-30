
namespace Nox.Core.Interfaces.Database;

public interface IEntityAttribute : IMetaBase
{
    bool CanFilter { get; set; }
    bool CanSort { get; set; }
    object? Default { get; set; }
    string[] DefaultFromParents { get; set; }
    string DefaultFromParentsJson { get; set; }
    string Description { get; set; }
    string Formula { get; set; }
    bool IsAutoNumber { get; set; }
    bool IsForeignKey { get; set; }
    bool IsPrimaryKey { get; set; }
    bool IsRequired { get; set; }
    bool IsTemporalOnly { get; set; }
    bool IsUnicode { get; set; }
    int MaxValue { get; set; }
    int MaxWidth { get; set; }
    int MinValue { get; set; }
    int MinWidth { get; set; }
    string Name { get; set; }
    int Precision { get; set; }
    string Type { get; set; }

    bool ApplyDefaults();
    bool IsDateTimeType();
    bool IsMappedAttribute();
    Type NetDataType();
}
