
using Nox.Core.Components;
using System.Collections.ObjectModel;

namespace Nox.Core.Configuration;

public class TeamConfiguration : MetaBase
{
    public List<TeamMemberConfiguration> Developers { get; set; } = new ();
}

