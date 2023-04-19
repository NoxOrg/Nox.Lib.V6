using Nox.Core.Interfaces.Messaging;
using Nox.Core.Interfaces.Secrets;

namespace Nox.Core.Models;

internal class Secret : ISecret
{
    public ICollection<SecretProvider>? Providers {
        get => Providers?.ToList<SecretProvider>();
        set => Providers = value;
    }

    public ISecretsValidFor? ValidFor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
