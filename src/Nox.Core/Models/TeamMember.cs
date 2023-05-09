using Nox.Core.Components;
using Nox.Core.Interfaces.VersionControl;

namespace Nox.Core.Models;

public sealed class TeamMember: MetaBase, ITeamMember
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobilePhoneNumber { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
    public bool IsProductOwner { get; set; }
}