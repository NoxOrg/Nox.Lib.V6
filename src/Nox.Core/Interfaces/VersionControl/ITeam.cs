namespace Nox.Core.Interfaces.VersionControl;

public interface ITeam: IMetaBase
{
    ICollection<ITeamMember>? Developers { get; set; }
}