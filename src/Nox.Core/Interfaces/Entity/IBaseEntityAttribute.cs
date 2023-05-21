
namespace Nox.Core.Interfaces.Entity;

public interface IBaseEntityAttribute : IMetaBase
{
    string Name { get; set; }
    string Description { get; set; }

    string Type { get; set; }

    bool IsAutoNumber { get; set; }
    bool IsRequired { get; set; }
    bool IsUnicode { get; set; }
    int MaxWidth { get; set; }
    int MinWidth { get; set; }
    int Precision { get; set; }

    Type NetDataType();
}
