using System.Runtime.Serialization;

namespace Nox.Core.Exceptions;

[Serializable]
public class NoxYamlException : Exception
{
    public NoxYamlException()
    {
    }

    public NoxYamlException(string? message)
        : base(message)
    {
    }

    public NoxYamlException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected NoxYamlException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}