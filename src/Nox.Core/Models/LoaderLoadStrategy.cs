using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Components;

namespace Nox.Core.Models
{
    public sealed class LoaderLoadStrategy : MetaBase
    {
        public string Type { get; set; } = string.Empty;
        [NotMapped]
        public string[] Columns { get; set; } = Array.Empty<string>();
        public string ColumnsJson { get => string.Join('|', Columns.ToArray()); set => Columns = value.Split('|'); }

    }
}
