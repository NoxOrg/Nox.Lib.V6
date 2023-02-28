using Nox.Core.Components;
using Nox.Core.Interfaces.Dto;

namespace Nox.Core.Models;

public class DtoAttribute: MetaBase, IDtoAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";

    public bool ApplyDefaults()
    {
        Type = Type.ToLower();
        return true;
    }

    public bool IsDateTimeType() => "|date|time|timespan|datetime|".Contains($"|{Type}|");

    public Type NetDataType()
    {
        return Type switch
        {
            "string" => typeof(String),
            "varchar" => typeof(String),
            "nvarchar" => typeof(String),
            "char" => typeof(String),
            "url" => typeof(String),
            "email" => typeof(String),
            "guid" => typeof(Nullable<Guid>),
            "date" => typeof(Nullable<DateTime>),
            "time" => typeof(Nullable<DateTime>),
            "timespan" => typeof(Nullable<DateTime>),
            "datetime" => typeof(Nullable<DateTimeOffset>),
            "bool" => typeof(Nullable<Boolean>),
            "boolean" => typeof(Nullable<Boolean>),
            "object" => typeof(Object),
            "int" => typeof(Nullable<Int32>),
            "uint" => typeof(Nullable<UInt32>),
            "tinyint" => typeof(Nullable<Int16>),
            "bigint" => typeof(Nullable<Int64>),
            "money" => typeof(Nullable<Decimal>),
            "smallmoney" => typeof(Nullable<Decimal>),
            "decimal" => typeof(Nullable<Decimal>),
            "real" => typeof(Nullable<Single>),
            "float" => typeof(Nullable<Single>),
            "bigreal" => typeof(Nullable<Double>),
            "bigfloat" => typeof(Nullable<Double>),
            _ => typeof(String)
        };

    }
}