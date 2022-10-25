using FluentValidation;
using Nox.Dynamic.DatabaseProviders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nox.Dynamic.MetaData;

public class MessageBusBase : MetaBase, IServiceMessageBus
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? ConnectionVariable { get; set; }

    [NotMapped]
    public IMessageBusProvider? MessageBusProvider { get; set; }

    internal virtual bool ApplyDefaults(ServiceValidationInfo info)
    {
        var isValid = true;

        if (string.IsNullOrEmpty(ConnectionString))
        {
            if (!string.IsNullOrEmpty(ConnectionVariable))
            {
                ConnectionString = info.ConfigurationVariables[ConnectionVariable];
            }
        }

        Provider = Provider.Trim().ToLower();

        switch (Provider)
        {
            case "azureservicebus":
                MessageBusProvider = new AzureServiceBusMessageBusProvider(this);
                break;

            case "rabbitmq":
                MessageBusProvider = new RabbitMqMessageBusProvider(this);
                break;

            default:
                isValid = false;
                break;
        }

        return isValid;
    }
}

internal class MessageBusValidator : AbstractValidator<MessageBusBase>
{
    public MessageBusValidator(ServiceValidationInfo info)
    {

        RuleFor(mb => mb.ApplyDefaults(info))
            .NotEqual(false)
            .WithMessage(mb => $"[{info.ServiceName}] MessageBus provider '{mb.Provider}' defined in {mb.DefinitionFileName} is not supported");

    }
}