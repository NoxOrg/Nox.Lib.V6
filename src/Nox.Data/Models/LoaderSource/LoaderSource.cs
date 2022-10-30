namespace Nox.Data;

public sealed class LoaderSource : DatabaseBase
{
    public string Query { get; set; } = string.Empty;
    public int MinimumExpectedRecords { get; set; } = 0;
}

public class LoaderSourceValidator : DatabaseValidator
{
    public LoaderSourceValidator(ServiceValidationInfo info) : base(info) { }
}
