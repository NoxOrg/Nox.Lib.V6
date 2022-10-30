
namespace Nox.Data
{
    public interface IServiceDatabase
    {
        string? ConnectionString { get; set; }
        string? ConnectionVariable { get; set; }
        string Name { get; set; }
        string Options { get; set; }
        string Password { get; set; }
        string Provider { get; set; }
        string Server { get; set; }
        public int Port { get; set; }
        string User { get; set; }
        public IDatabaseProvider? DatabaseProvider { get; set; }

    }
}