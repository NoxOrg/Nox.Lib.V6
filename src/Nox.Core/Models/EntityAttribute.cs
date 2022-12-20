using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Models;

public sealed class EntityAttribute : MetaBase, IEntityAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsAutoNumber { get; set; } = false;
    public bool IsForeignKey { get; set; } = false;
    public bool IsRequired { get; set; } = false;
    public bool IsUnicode { get; set; } = true;
    public bool IsTemporalOnly { get; set; } = false;
    public bool CanFilter { get; set; } = false;
    public bool CanSort { get; set; } = false;
    public int MinWidth { get; set; } = 0;
    public int MaxWidth { get; set; } = 512;
    public int MinValue { get; set; } = int.MinValue;
    public int MaxValue { get; set; } = int.MaxValue;
    public int Precision { get; set; } = 2;
    [NotMapped]
    public object? Default { get; set; }
    [NotMapped]
    public string[] DefaultFromParents { get; set; } = Array.Empty<string>();
    public string DefaultFromParentsJson { get => string.Join('|', DefaultFromParents.ToArray()); set => DefaultFromParents = value.Split('|'); }
    public string Formula { get; set; } = string.Empty;

    public bool IsMappedAttribute() => (string.IsNullOrEmpty(Formula) && !DefaultFromParents.Any());


    public bool ApplyDefaults()
    {

        if (MaxWidth < 1)
            MaxWidth = 512;

        Type = Type.ToLower();

        if (IsPrimaryKey)
            IsRequired = true;

        DefaultFromParents = DefaultFromParents.Where(p => p.Trim().Length > 0).ToArray();

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
            "guid" => IsRequired ? typeof(Guid) : typeof(Nullable<Guid>),
            "date" => IsRequired ? typeof(DateTime) : typeof(Nullable<DateTime>),
            "time" => IsRequired ? typeof(DateTime) : typeof(Nullable<DateTime>),
            "timespan" => IsRequired ? typeof(DateTime) : typeof(Nullable<DateTime>),
            "datetime" => IsRequired ? typeof(DateTimeOffset) : typeof(Nullable<DateTimeOffset>),
            "bool" => IsRequired ? typeof(Boolean) : typeof(Nullable<Boolean>),
            "boolean" => IsRequired ? typeof(Boolean) : typeof(Nullable<Boolean>),
            "object" => typeof(Object),
            "int" => IsRequired ? typeof(Int32) : typeof(Nullable<Int32>),
            "uint" => IsRequired ? typeof(UInt32) : typeof(Nullable<UInt32>),
            "tinyint" => IsRequired ? typeof(Int16) : typeof(Nullable<Int16>),
            "bigint" => IsRequired ? typeof(Int64) : typeof(Nullable<Int64>),
            "money" => IsRequired ? typeof(Decimal) : typeof(Nullable<Decimal>),
            "smallmoney" => IsRequired ? typeof(Decimal) : typeof(Nullable<Decimal>),
            "decimal" => IsRequired ? typeof(Decimal) : typeof(Nullable<Decimal>),
            "real" => IsRequired ? typeof(Single) : typeof(Nullable<Single>),
            "float" => IsRequired ? typeof(Single) : typeof(Nullable<Single>),
            "bigreal" => IsRequired ? typeof(Double) : typeof(Nullable<Double>),
            "bigfloat" => IsRequired ? typeof(Double) : typeof(Nullable<Double>),
            _ => typeof(String)
        };

    }
}


