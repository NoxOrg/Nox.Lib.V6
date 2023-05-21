namespace Nox.Core.Interfaces.Entity.Commands
{
    /// <summary>
    /// Represents the Nox Result.
    /// </summary>
    public class NoxCommandResult : INoxCommandResult
    {
        public bool IsSuccess { get; init; }

        public string? Message { get; init; }
    }
}
