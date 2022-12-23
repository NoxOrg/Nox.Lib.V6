using Nox.Core.Components;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Models;

public sealed class EntityMessageTarget: MetaBase, IEntityMessageTarget
{
    public string MessagingProvider { get; set; } = string.Empty;
}