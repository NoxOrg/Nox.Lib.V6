using Nox.Dynamic.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Dynamic.MetaData;

public sealed class ServiceMessageBus : MessageBusBase, IServiceMessageBus { }

internal class ServiceMessageBusValidator : MessageBusValidator
{
    public ServiceMessageBusValidator() : base() { }
}
