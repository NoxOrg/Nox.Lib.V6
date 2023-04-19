using Nox.Core.Configuration.Secrets;
using Nox.Core.Models;

namespace Nox.Core.Interfaces.Secrets
{
    public interface ISecret
    {
        ICollection<SecretProvider>? Providers { get; set; }
        ISecretsValidFor? ValidFor { get; set; }
    }
}