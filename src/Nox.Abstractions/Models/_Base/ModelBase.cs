using System.ComponentModel.DataAnnotations.Schema;

namespace Nox;

public class ModelBase : IModelBase
{
    public int Id { get; set; }

    [NotMapped]
    public string DefinitionFileName { get; set; } = String.Empty;
}
