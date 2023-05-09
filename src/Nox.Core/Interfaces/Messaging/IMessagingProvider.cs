namespace Nox.Core.Interfaces.Messaging;

public interface IMessagingProvider: IMetaBase
{
    string Name { get; set; }
    bool IsHeartbeat { get; set; }
    string Provider { get; set; }
    string? ConnectionString { get; set; }
    string? ConnectionVariable { get; set; }
    string? AccessKey { get; set; }
    string? SecretKey { get; set; }
    bool ApplyDefaults();

}