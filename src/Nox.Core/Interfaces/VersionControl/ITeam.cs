namespace Nox.Core.Interfaces.VersionControl;

public interface ITeam: IMetaBase
{
    public ICollection<ITeamMember>? Developers { get; set; }
}