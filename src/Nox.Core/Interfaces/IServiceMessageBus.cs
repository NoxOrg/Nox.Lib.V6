namespace Nox.Core.Interfaces
{
    public interface IServiceMessageBus
    {
        string Name { get; set; }
        string Provider { get; set; }
        string? ConnectionString { get; set; }
        string? ConnectionVariable { get; set; }
        //IMessageBusProvider? MessageBusProvider { get; set; }
    }
}