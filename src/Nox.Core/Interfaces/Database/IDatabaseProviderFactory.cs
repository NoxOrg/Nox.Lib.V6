namespace Nox.Core.Interfaces
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider Create(string provider);
    }
}