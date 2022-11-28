using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Nox;
using Nox.Messaging;

namespace Samples.Cli.Commands;

public class HeartbeatConsumer : IConsumer<HeartbeatMessage>
{
    readonly ILogger<HeartbeatConsumer> _logger;
    public HeartbeatConsumer(ILogger<HeartbeatConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HeartbeatMessage> context)
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

public class CurrencyCreatedEventConsumer : IConsumer<CurrencyCreatedDomainEvent>
{
    readonly ILogger<CurrencyCreatedEventConsumer> _logger;
    
    public CurrencyCreatedEventConsumer(ILogger<CurrencyCreatedEventConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<CurrencyCreatedDomainEvent> context)
    {
        _logger.LogInformation("Country Created: {Text}", JsonSerializer.Serialize(context.Message.Payload));
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

