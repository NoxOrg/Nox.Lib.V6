using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Secrets;

namespace Nox.Core.Models;

internal class Secret : ISecret
{
    ICollection<ISecretProvider>? ISecret.Providers
    {
        get => Providers?.ToList<ISecretProvider>();
        set => Providers = value as ICollection<SecretProvider>;
    }

    public ICollection<SecretProvider>? Providers;

    public ISecretsValidFor? ValidFor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
