using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.Dto
{
    public class EntityAttribute
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
        public object? Default { get; set; }
        public string[] DefaultFromParents { get; set; } = Array.Empty<string>();
        public string Formula { get; set; } = string.Empty;



        public Type NetDataType()
        {
            var propType = Type?.ToLower() ?? "string";

            return propType switch
            {
                "string" => typeof(String),
                "varchar" => typeof(String),
                "nvarchar" => typeof(String),
                "char" => typeof(String),
                "guid" => typeof(Guid),
                "url" => typeof(String),
                "email" => typeof(String),
                "date" => typeof(DateTime),
                "time" => typeof(DateTime),
                "timespan" => typeof(TimeSpan),
                "datetime" => typeof(DateTimeOffset),
                "bool" => typeof(Boolean),
                "boolean" => typeof(Boolean),
                "object" => typeof(Object),
                "int" => typeof(Int32),
                "uint" => typeof(UInt32),
                "tinyint" => typeof(Int16),
                "bigint" => typeof(Int64),
                "money" => typeof(Decimal),
                "smallmoney" => typeof(Decimal),
                "decimal" => typeof(Decimal),
                "real" => typeof(Single),
                "float" => typeof(Single),
                "bigreal" => typeof(Double),
                "bigfloat" => typeof(Double),
                _ => typeof(String)
            };

        }


    }
}
