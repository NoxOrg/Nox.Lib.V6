
namespace Nox.Data;

public sealed class ServiceDatabase : DatabaseBase, IServiceDatabase  {}

public class ServiceDatabaseValidator : DatabaseValidator
{
    public ServiceDatabaseValidator(ServiceValidationInfo info) : base(info) { }
}
