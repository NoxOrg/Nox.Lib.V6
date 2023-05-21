namespace Nox.Core.Interfaces.Entity.Commands
{
    /// <summary>
    /// Represents the Nox Result.
    /// </summary>
    public interface INoxCommandResult
    {
        public bool IsSuccess { get; }

        public string? Message { get; }
    }
}
