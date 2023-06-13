namespace Nox.Core.Interfaces.Database
{
    public interface IDataProviderFactory
    {
        IDataProvider Create(DataConnectionProvider provider);
    }
}