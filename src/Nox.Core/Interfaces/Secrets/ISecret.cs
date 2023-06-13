using Nox.Core.Models;

namespace Nox.Core.Interfaces.Secrets
{
    public interface ISecret
    {
        ICollection<ISecretProvider>? Providers { get; set; }
        ISecretsValidFor? ValidFor { get; set; }
    }
}