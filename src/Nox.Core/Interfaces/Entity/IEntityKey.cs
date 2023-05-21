namespace Nox.Core.Interfaces.Entity;

public interface IEntityKey : IBaseEntityAttribute
{
    string[] Entities { get; set; }

    bool IsComposite { get; }
}
