using Nox.Core.Components;
using Nox.Core.Interfaces.VersionControl;

namespace Nox.Core.Models;

public sealed class Team: MetaBase, ITeam
{
    public ICollection<TeamMember>? Developers { get; set; }
    
    ICollection<ITeamMember>? ITeam.Developers
    {
        get => Developers?.ToList<ITeamMember>();
        set => Developers = value as ICollection<TeamMember>;
    }
}
