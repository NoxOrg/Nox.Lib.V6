using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Configuration;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Messaging;

public class NoxMessenger: INoxMessenger
{
    private readonly ILogger _logger;
    private readonly IProjectConfiguration _config;
    private readonly IRabbitMqBus? _rabbitBus = null;
    private readonly IAzureBus? _azureBus = null;
    private readonly IAmazonBus? _amazonBus = null;
    private readonly IMediator? _mediator = null;
    
    public NoxMessenger(
        ILogger<NoxMessenger> logger,
        IProjectConfiguration config,
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

    public async Task SendMessageAsync<T>(IEnumerable<string> messageProviders, T message) where T : notnull
    {
        foreach (var messageProvider in messageProviders)
        {
            try
            {
                if (_config.MessagingProviders == null)
                {
                    throw new ConfigurationException("Cannot add messaging if messaging providers not present in configuration!");
                }

                var providerInstance = _config.MessagingProviders.First(p =>
                    p.Name != null && p.Name.Equals(messageProvider, StringComparison.OrdinalIgnoreCase)
                );

                if (providerInstance == null)
                {
                    throw new ConfigurationException($"Messaging provider '{messageProvider}' is referneced but not present in the NOX configuration");
                }
                
                var messageType = message.GetType().Name;

                switch (providerInstance.Provider.ToLower())
                {
                    case "rabbitmq":
                        await _rabbitBus!.Publish(message);
                        _logger.LogInformation($"{messageType} sent to RabbitMq bus.");
                        break;
                    case "azureservicebus":
                        await _azureBus!.Publish(message);
                        _logger.LogInformation($"{messageType} sent to Azure bus.");
                        break;
                    case "amazonsqs":
                        await _amazonBus!.Publish(message);
                        _logger.LogInformation($"{messageType} sent to Amazon bus.");
                        break;
                    case "mediator":
                        await _mediator!.Publish(message);
                        _logger.LogInformation($"{messageType} sent to Mediator.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }

    public async Task SendMessage(IEnumerable<IMessageTarget> messageTargets, object message)
    {
        await SendMessageAsync(messageTargets.Select(m => m.MessagingProvider), message);
    }

    public async Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken)
    {
        foreach (var provider in _config.MessagingProviders!)
        {
            try
            {
                switch (provider.Provider!.ToLower())
                {
                    case "rabbitmq":
                        await _rabbitBus!.Publish(message, stoppingToken);
                        _logger.LogInformation($"Heartbeat sent to RabbitMq bus.");
                        break;
                    case "azureservicebus":
                        await _azureBus!.Publish(message, stoppingToken);
                        _logger.LogInformation($"Heartbeat sent to Azure bus.");
                        break;
                    case "amazonsqs":
                        await _amazonBus!.Publish(message, stoppingToken);
                        _logger.LogInformation($"Heartbeat sent to Amazon bus.");
                        break;
                    case "mediator":
                        if (_mediator != null)
                        {
                            await _mediator.Publish(message, stoppingToken)!;
                            _logger.LogInformation($"Heartbeat sent to Mediator.");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
