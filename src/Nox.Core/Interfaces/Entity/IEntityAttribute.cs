
namespace Nox.Core.Interfaces.Entity;

public interface IEntityAttribute : IBaseEntityAttribute
{
    bool CanFilter { get; set; }
    bool CanSort { get; set; }
    object? Default { get; set; }
    string[] DefaultFromParents { get; set; }
    string DefaultFromParentsJson { get; set; }
    string Formula { get; set; }
    bool IsForeignKey { get; set; }
    bool IsTemporalOnly { get; set; }
    int MaxValue { get; set; }
    int MinValue { get; set; }

    bool ApplyDefaults();
    bool IsDateTimeType();
    bool IsMappedAttribute();
}
