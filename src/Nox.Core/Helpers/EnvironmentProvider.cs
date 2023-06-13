namespace Nox.Core.Helpers;

internal sealed class EnvironmentProvider : IEnvironmentProvider
{
    public string? GetEnvironmentVariable(string variable)
    {
        return Environment.GetEnvironmentVariable(variable);
    }
}