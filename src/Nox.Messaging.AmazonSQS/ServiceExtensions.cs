using MassTransit;

namespace Nox.Messaging.AmazonSQS;

public static class ServiceExtensions
{
    public static void UseAmazonSqs(this IBusRegistrationConfigurator configurator, string hostAddress, string accessKey, string secretKey)
    {
        configurator.UsingAmazonSqs((context, cfg) =>
        {
            cfg.Host(hostAddress, h =>
            {
                h.AccessKey(accessKey);
                h.SecretKey(secretKey);
            });
            cfg.ConfigureEndpoints(context);
        });
    }
}