namespace Nox.Data
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider Create(string provider);
    }
}