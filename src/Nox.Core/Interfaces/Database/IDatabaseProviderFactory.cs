namespace Nox.Core.Interfaces.Database
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider Create(string provider);
    }
}