using System.Runtime.Serialization;

namespace Nox;

[Serializable]
public class NoxException :
    Exception
{
    public NoxException()
    {
    }

    public NoxException(string? message)
        : base(message)
    {
    }

    public NoxException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected NoxException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

