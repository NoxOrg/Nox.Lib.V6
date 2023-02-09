
using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class TeamMemberConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobilePhoneNumber { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
}

