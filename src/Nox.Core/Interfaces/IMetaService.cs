namespace Nox.Core.Interfaces;

public interface IMetaService
{
    string Name { get; set; }
    string Description { get; set; }
    string KeyVaultUri { get; set; }
    IServiceDatabase? Database { get; set; }
    IServiceMessageBus? MessageBus { get; set; }
    ICollection<IEntity>? Entities { get; set; }
    ICollection<ILoader>? Loaders { get; set; }
    ICollection<IApi>? Apis { get; set; }
}