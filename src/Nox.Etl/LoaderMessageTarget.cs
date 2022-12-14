using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Etl;

namespace Nox.Etl;

public sealed class LoaderMessageTarget: MetaBase, ILoaderMessageTarget
{
    public string MessagingProvider { get; set; } = string.Empty;
}