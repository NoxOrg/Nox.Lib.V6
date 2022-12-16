using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;
using Nox.Messaging;

namespace Samples.Cli.Commands;

// Heartbeat Events

public class HeartbeatConsumer : IConsumer<IHeartbeatMessage>
{
    readonly ILogger<HeartbeatConsumer> _logger;

    public HeartbeatConsumer(ILogger<HeartbeatConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<IHeartbeatMessage> context)
    {
        _logger.LogInformation("Received Heartbeat: {Text}", context.Message.Value);
        return Task.CompletedTask;
    }
}

public class HeartbeatConsumerDefinition : ConsumerDefinition<HeartbeatConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<HeartbeatConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

// Country Created Events

public class CountryCreatedEventConsumer : IConsumer<CountryCreatedDomainEvent>
{
    readonly ILogger<CountryCreatedEventConsumer> _logger;
    
    public CountryCreatedEventConsumer(ILogger<CountryCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CountryCreatedDomainEvent> context)
    {
        _logger.LogInformation("Country Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class CountryCreatedEventConsumerDefinition : ConsumerDefinition<CountryCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CountryCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

// Currency Created Events

public class CurrencyCreatedEventConsumer : IConsumer<CurrencyCreatedDomainEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;
    
    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyCreatedDomainEvent> context)
    {
        _logger.LogInformation("Currency Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class CurrencyCreatedEventConsumerDefinition : ConsumerDefinition<CurrencyCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CurrencyCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

// Currency Updated Events

public class CurrencyUpdatedEventConsumer : IConsumer<CurrencyUpdatedDomainEvent>
{
    readonly ILogger<CurrencyUpdatedEventConsumer> _logger;

    public CurrencyUpdatedEventConsumer(ILogger<CurrencyUpdatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CurrencyUpdatedDomainEvent> context)
    {
        _logger.LogInformation("Currency Updated: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class CurrencyUpdatedEventConsumerDefinition : ConsumerDefinition<CurrencyUpdatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CurrencyUpdatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}


// Workplace Created Events

public class WorkplaceCreatedEventConsumer : IConsumer<WorkplaceCreatedDomainEvent>
{
    readonly ILogger<WorkplaceCreatedEventConsumer> _logger;
    
    public WorkplaceCreatedEventConsumer(ILogger<WorkplaceCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<WorkplaceCreatedDomainEvent> context)
    {
        _logger.LogInformation("Workplace Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class WorkplaceCreatedEventConsumerDefinition : ConsumerDefinition<WorkplaceCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<WorkplaceCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

// Brand Created Events

public class BrandCreatedEventConsumer: IConsumer<BrandCreatedDomainEvent>
{
    readonly ILogger<BrandCreatedEventConsumer> _logger;
    
    public BrandCreatedEventConsumer(ILogger<BrandCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<BrandCreatedDomainEvent> context)
    {
        _logger.LogInformation("Brand Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class BrandCreatedEventConsumerDefinition : ConsumerDefinition<BrandCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<BrandCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

// Platform Tenant Created Events

public class PlatformTenantCreatedEventConsumer: IConsumer<PlatformTenantCreatedDomainEvent>
{
    readonly ILogger<PlatformTenantCreatedEventConsumer> _logger;
    
    public PlatformTenantCreatedEventConsumer(ILogger<PlatformTenantCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<PlatformTenantCreatedDomainEvent> context)
    {
        _logger.LogInformation("Brand Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
        return Task.CompletedTask;
    }
}

public class PlatformTenantCreatedEventConsumerDefinition : ConsumerDefinition<PlatformTenantCreatedEventConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<PlatformTenantCreatedEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}

