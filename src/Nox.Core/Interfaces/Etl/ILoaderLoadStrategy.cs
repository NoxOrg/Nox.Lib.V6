using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Core.Interfaces;

public interface ILoaderLoadStrategy
{
    string Type { get; set; }
    [NotMapped]
    string[] Columns { get; set; }
    string ColumnsJson { get => string.Join('|', Columns.ToArray()); set => Columns = value.Split('|'); }
}