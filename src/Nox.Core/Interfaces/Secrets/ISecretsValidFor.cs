namespace Nox.Core.Interfaces.Secrets
{
    public interface ISecretsValidFor
    {
        int? Days { get; set; }
        int? Hours { get; set; }
        int? Minutes { get; set; }
        int? Seconds { get; set; }
    }
}