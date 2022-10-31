
namespace Nox.Data;

public sealed class ServiceDatabase : DatabaseBase, IServiceDatabase  {}

public class ServiceDatabaseValidator : DatabaseValidator
{
    public ServiceDatabaseValidator() : base() { }
}
