
namespace Nox.Core.Interfaces.Database
{
    public interface IServiceDataSource
    {
        string? ConnectionString { get; set; }
        string Name { get; set; }
        string Options { get; set; }
        string Password { get; set; }
        DataConnectionProvider Provider { get; set; }
        string Server { get; set; }
        public int Port { get; set; }
        string User { get; set; }
        public IDataProvider? DataProvider { get; set; }
        bool ApplyDefaults();
    }
}