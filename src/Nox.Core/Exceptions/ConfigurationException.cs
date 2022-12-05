using System.Runtime.Serialization;
using Nox.Core.Configuration;
using Nox.Core.Models;

namespace Nox.Core.Exceptions;

[Serializable]
public class ConfigurationException :
    NoxException
{
    public ConfigurationException()
    {
    }

    public ConfigurationException(IEnumerable<IValidationResult> results, string message)
        : base(message)
    {
        Results = results;
    }

    public ConfigurationException(IEnumerable<IValidationResult> results, string message, Exception innerException)
        : base(message, innerException)
    {
        Results = results;
    }

    public ConfigurationException(string message)
        : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected ConfigurationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public IEnumerable<IValidationResult> Results { get; protected set; } = Array.Empty<IValidationResult>();
}

