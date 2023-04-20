
using Nox.Core.Interfaces.Secrets;

namespace Nox.Core.Models;

internal class SecretsValidFor : ISecretsValidFor
{
    public int? Days { get; set; }
    public int? Hours { get; set; }
    public int? Minutes { get; set; }
    public int? Seconds { get; set; }
}
