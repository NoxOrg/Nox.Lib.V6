using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Enumerations;
using Nox.Data;

namespace Samples.Api.Consumers;

public class CountryCreatedEventConsumer: IConsumer<CountryCreatedDomainEvent>
{
    readonly ILogger<CountryCreatedEventConsumer> _logger;

    public CountryCreatedEventConsumer(ILogger<CountryCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CountryCreatedDomainEvent> context)
    {
        _logger.LogInformation("Received CountryCreatedDomainEvent from {message}: {@payload}", context.Message, context.Message.Payload);
        return Task.CompletedTask;
    }
}