using System.Runtime.Serialization;

namespace Nox.Core.Exceptions;

[Serializable]
public class NoxIntegrationException : Exception
{
    public NoxIntegrationException()
    {
    }

    public NoxIntegrationException(string? message)
        : base(message)
    {
    }

    public NoxIntegrationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected NoxIntegrationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}