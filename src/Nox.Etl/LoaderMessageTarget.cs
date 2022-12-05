using Nox.Core.Components;
using Nox.Core.Interfaces;

namespace Nox.Etl;

public sealed class LoaderMessageTarget: MetaBase, ILoaderMessageTarget
{
    public string Name { get; set; } = string.Empty;
}