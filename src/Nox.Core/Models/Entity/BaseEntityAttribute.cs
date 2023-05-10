using Nox.Core.Components;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Models.Entity;

public abstract class BaseEntityAttribute : MetaBase, IBaseEntityAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string";

    public virtual bool IsRequired { get; set; } = false;

    public int Precision { get; set; } = 2;

    public bool IsAutoNumber { get; set; } = false;
    public int MinWidth { get; set; } = 0;
    public int MaxWidth { get; set; } = 512;
    public bool IsUnicode { get; set; } = true;

    public Type NetDataType()
    {
        return Type switch
        {
            "string" => typeof(string),
            "varchar" => typeof(string),
            "nvarchar" => typeof(string),
            "char" => typeof(string),
            "url" => typeof(string),
            "email" => typeof(string),
            "guid" => IsRequired ? typeof(Guid) : typeof(Guid?),
            "date" => IsRequired ? typeof(DateTime) : typeof(DateTime?),
            "time" => IsRequired ? typeof(DateTime) : typeof(DateTime?),
            "timespan" => IsRequired ? typeof(DateTime) : typeof(DateTime?),
            "datetime" => IsRequired ? typeof(DateTimeOffset) : typeof(DateTimeOffset?),
            "bool" => IsRequired ? typeof(bool) : typeof(bool?),
            "boolean" => IsRequired ? typeof(bool) : typeof(bool?),
            "object" => typeof(object),
            "int" => IsRequired ? typeof(int) : typeof(int?),
            "uint" => IsRequired ? typeof(uint) : typeof(uint?),
            "tinyint" => IsRequired ? typeof(short) : typeof(short?),
            "bigint" => IsRequired ? typeof(long) : typeof(long?),
            "money" => IsRequired ? typeof(decimal) : typeof(decimal?),
            "smallmoney" => IsRequired ? typeof(decimal) : typeof(decimal?),
            "decimal" => IsRequired ? typeof(decimal) : typeof(decimal?),
            "real" => IsRequired ? typeof(float) : typeof(float?),
            "float" => IsRequired ? typeof(float) : typeof(float?),
            "bigreal" => IsRequired ? typeof(double) : typeof(double?),
            "bigfloat" => IsRequired ? typeof(double) : typeof(double?),
            _ => typeof(string)
        };
    }
}