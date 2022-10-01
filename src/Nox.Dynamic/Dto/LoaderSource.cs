namespace Nox.Dynamic.Dto
{
    internal class LoaderSource : ServiceDatabase
    {
        public string Query { get; set; } = string.Empty;
        public int MinimumExpectedRecords { get; set; } = 0;
    }
}
