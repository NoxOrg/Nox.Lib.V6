namespace Nox.Core.Interfaces.Secrets
{
    public interface ISecretProvider
    {
        string Provider { get; set; }
        string Url { get; set; }
    }
}