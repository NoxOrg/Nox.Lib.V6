using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces;

namespace Nox.Messaging;

public class NoxMessenger: INoxMessenger
{
    private readonly ILogger _logger;
    private INoxConfiguration _config;
    private readonly IRabbitMqBus? _rabbitBus = null;
    private readonly IAzureBus? _azureBus = null;
    private readonly IAmazonBus? _amazonBus = null;
    private readonly IMediator? _mediator = null;
    
    public NoxMessenger(
        ILogger<NoxMessenger> logger,
        INoxConfiguration config,
        IRabbitMqBus? rabbitBus = null,
        IAzureBus? azureBus = null,
        IAmazonBus? amazonBus = null,
        IMediator? mediator = null)
    {
        _logger = logger;
        _config = config;
        _rabbitBus = rabbitBus;
        _azureBus = azureBus;
        _amazonBus = amazonBus;
        _mediator = mediator;
    }

    public async Task SendMessage(ILoader loader, object message)
    {
        if (loader.Messaging != null)
        {
            foreach (var loaderMsg in loader.Messaging!)
            {
                if (_config.MessagingProviders == null) throw new ConfigurationException("Cannot add messaging if messaging providers not present in configuration!");
                var providerInstance = _config.MessagingProviders.First(p => p.Name == loaderMsg.Name);
                switch (providerInstance.Provider!.ToLower())
                {
                    case "rabbitmq":
                        _logger.LogInformation($"Publishing {message.GetType().Name} to RabbitMq bus.");
                        await _rabbitBus!.Publish(message);
                        break;
                    case "azureservicebus":
                        _logger.LogInformation($"Publishing {message.GetType().Name} to Azure bus.");
                        await _azureBus!.Publish(message);
                        break;
                    case "amazonsqs":
                        _logger.LogInformation($"Publishing {message.GetType().Name} to Amazon bus.");
                        await _amazonBus!.Publish(message);
                        break;
                    case "mediator":
                        _logger.LogInformation($"Publishing {message.GetType().Name} to Mediator.");
                        await _mediator!.Publish(message);
                        break;
                }
            }
        }
    }

    public async Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken)
    {
        foreach (var provider in _config.MessagingProviders!)
        {
            if (provider.IsHeartbeat)
            {
                switch (provider.Provider.ToLower())
                {
                    case "rabbitmq":
                        _logger.LogInformation($"Publishing heartbeat to RabbitMq bus.");
                        await _rabbitBus!.Publish(message, stoppingToken);
                        break;
                    case "azureservicebus":
                        _logger.LogInformation($"Publishing heartbeat to Azure bus.");
                        await _azureBus!.Publish(message, stoppingToken);
                        break;
                    case "amazonsqs":
                        _logger.LogInformation($"Publishing heartbeat to Amazon bus.");
                        await _amazonBus!.Publish(message, stoppingToken);
                        break;
                    case "mediator":
                        _logger.LogInformation($"Publishing heartbeat to Mediator.");
                        await _mediator!.Publish(message, stoppingToken);
                        break;
                }
            }
        }
    }
}