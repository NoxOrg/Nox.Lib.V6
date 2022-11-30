using System.ComponentModel.DataAnnotations.Schema;
using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Etl
{
    public sealed class LoaderLoadStrategy : MetaBase, ILoaderLoadStrategy
    {
        public string Type { get; set; } = string.Empty;
        [NotMapped]
        public string[] Columns { get; set; } = Array.Empty<string>();
        public string ColumnsJson { get => string.Join('|', Columns.ToArray()); set => Columns = value.Split('|'); }

    }
}
