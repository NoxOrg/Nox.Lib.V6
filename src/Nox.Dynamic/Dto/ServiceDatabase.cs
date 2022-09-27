namespace Nox.Dynamic.Dto
{
    internal class ServiceDatabase
    {
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = "SqlServer";
        public string Server { get; set; } = "localhost";
        public string User { get; set; } = "user";
        public string Password { get; set; } = "password";
        public string Options { get; set; } = "";
        public string? ConnectionString { get; set; }
    }
}
