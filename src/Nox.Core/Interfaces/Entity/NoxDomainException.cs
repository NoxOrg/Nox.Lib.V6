
namespace Nox.Core.Interfaces.Entity
{
    public class NoxDomainException : Exception
    {
        public NoxDomainException(string message) 
            : base(message) { }
    }
}