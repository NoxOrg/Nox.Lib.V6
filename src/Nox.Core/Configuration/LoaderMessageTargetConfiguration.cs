using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class LoaderMessageTargetConfiguration: MetaBase
{
    public string MessagingProvider { get; set; } = string.Empty;
}