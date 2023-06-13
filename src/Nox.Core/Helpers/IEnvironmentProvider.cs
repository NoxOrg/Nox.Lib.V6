namespace Nox.Core.Helpers;

/// <summary>
/// Abstract Environment Static Class
/// </summary>
public interface IEnvironmentProvider
{
    /// <summary>Retrieves the value of an environment variable from the current process.</summary>
    /// <param name="variable">The name of the environment variable.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="variable" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to perform this operation.</exception>
    /// <returns>The value of the environment variable specified by <paramref name="variable" />, or <see langword="null" /> if the environment variable is not found.</returns>
    string? GetEnvironmentVariable(string variable);
}