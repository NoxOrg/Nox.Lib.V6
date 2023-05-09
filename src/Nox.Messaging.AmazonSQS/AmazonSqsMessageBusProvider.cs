using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Messaging;

namespace Nox.Messaging.AmazonSQS;

public class AmazonSqsMessageBusProvider: IMessageBusProvider
{
    private string _connectionString;
    private string _accessKey;
    private string _secretKey;

    public AmazonSqsMessageBusProvider(IMessagingProvider provider)
    {
        _connectionString = provider.ConnectionString!;
        _accessKey = provider.AccessKey!;
        _secretKey = provider.SecretKey!;
    }
}