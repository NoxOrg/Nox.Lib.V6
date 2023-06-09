using Nox.Core.Interfaces.Database;

namespace Nox.Data;

public class NoxEntityStore: IServiceDataSource
{
    public int Id { get; set; }
    public string? ConnectionString { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string User { get; set; } = string.Empty;
    public IDataProvider? DataProvider { get; set; }
    public bool ApplyDefaults()
    {
        throw new NotImplementedException();
    }
}