namespace Nox.Core.Interfaces.Database
{
    public interface IDataProviderFactory
    {
        IDataProvider Create(string provider);
    }
}