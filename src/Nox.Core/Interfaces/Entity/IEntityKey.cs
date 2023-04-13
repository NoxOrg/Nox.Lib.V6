namespace Nox.Core.Interfaces.Entity;

public interface IEntityKey : IMetaBase, IBaseEntityAttribute
{
    string[] Entities { get; set; }

    bool IsComposite { get; }
}
