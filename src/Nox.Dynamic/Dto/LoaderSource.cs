namespace Nox.Dynamic.Dto
{
    internal class LoaderSource
    {
        public string ConnectionVariable { get; set; } = string.Empty;
        public string DatabaseProvider { get; set; } = "SqlServer";
        public string Query { get; set; } = string.Empty;
        public int MinimumExpectedRecords { get; set; } = 0;
    }
}
