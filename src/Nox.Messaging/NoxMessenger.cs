using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;
using Nox.Solution;

namespace Nox.Messaging;

public class NoxMessenger: INoxMessenger
{
    private readonly ILogger _logger;
    private readonly NoxSolution _solution;
    private readonly IRabbitMqBus? _rabbitBus = null;
    private readonly IAzureBus? _azureBus = null;
    private readonly IAmazonBus? _amazonBus = null;
    private readonly IMediator? _mediator = null;
    
    public NoxMessenger(
        ILogger<NoxMessenger> logger,
        NoxSolution solution,
        IRabbitMqBus? rabbitBus = null,
        IAzureBus? azureBus = null,
        IAmazonBus? amazonBus = null,
        IMediator? mediator = null)
    {
        _logger = logger;
        _rabbitBus = rabbitBus;
        _azureBus = azureBus;
        _amazonBus = amazonBus;
        _mediator = mediator;
    }

    public async Task SendMessageAsync<T>(IEnumerable<string> messageProviders, T message) where T : notnull
    {
        try
        {
            var messageType = message.GetType().Name;
            await _mediator!.Publish(message);
            _logger.LogInformation("{MessageType} sent to Mediator", messageType);

            var ieServer = _solution.Infrastructure?.Messaging?.IntegrationEventServer;
            if (ieServer != null)
            {
                switch (ieServer.Provider)
                {
                    case MessagingServerProvider.RabbitMq:
                        await _rabbitBus!.Publish(message);
                        _logger.LogInformation("{MessageType} sent to RabbitMq bus", messageType);
                        break;
                    case MessagingServerProvider.AzureServiceBus:
                        await _azureBus!.Publish(message);
                        _logger.LogInformation("{MessageType} sent to Azure bus", messageType);
                        break;
                    case MessagingServerProvider.AmazonSqs:
                        await _amazonBus!.Publish(message);
                        _logger.LogInformation("{MessageType} sent to Amazon bus", messageType);
                        break;
                        
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    public async Task SendMessage(IEnumerable<IMessageTarget> messageTargets, object message)
    {
        await SendMessageAsync(messageTargets.Select(m => m.MessagingProvider), message);
    }

    public async Task SendHeartbeat(IHeartbeatMessage message, CancellationToken stoppingToken)
    {
        try
        {
            await _mediator!.Publish(message, stoppingToken);
            _logger.LogInformation("Heartbeat sent to Mediator");

            var ieServer = _solution.Infrastructure?.Messaging?.IntegrationEventServer;
            if (ieServer != null)
            {
                switch (ieServer.Provider)
                {
                    case MessagingServerProvider.RabbitMq:
                        await _rabbitBus!.Publish(message, stoppingToken);
                        _logger.LogInformation("Heartbeat sent to RabbitMq bus");
                        break;
                    case MessagingServerProvider.AzureServiceBus:
                        await _azureBus!.Publish(message, stoppingToken);
                        _logger.LogInformation("Heartbeat sent to Azure bus");
                        break;
                    case MessagingServerProvider.AmazonSqs:
                        await _amazonBus!.Publish(message, stoppingToken);
                        _logger.LogInformation("Heartbeat sent to Amazon bus");
                        break;
                        
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}
