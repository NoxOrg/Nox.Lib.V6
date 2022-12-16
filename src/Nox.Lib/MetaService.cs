using FluentValidation;
using Nox.Core.Components;
using Nox.Core.Constants;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Api;
using Nox.Core.Interfaces.Database;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Interfaces.Messaging;
using Nox.Core.Validation;
using Nox.Data;
using Nox.Etl;
using Nox.Messaging;

namespace Nox.Lib;

public sealed class MetaService : MetaBase, IMetaService
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EndpointProvider { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = KeyVault.DefaultKeyVaultUri;

    IServiceDataSource? IMetaService.Database
    {
        get => Database;
        set => Database = value as ServiceDatabase;
    }
    public ServiceDatabase? Database { get; set; } = new();
    
    ICollection<IMessagingProvider>? IMetaService.MessagingProviders
    {
        get => MessagingProviders?.ToList<IMessagingProvider>();
        set => MessagingProviders = value as ICollection<MessagingProvider>;
    }
    
    public ICollection<MessagingProvider>? MessagingProviders { get; set; }

    ICollection<IServiceDataSource>? IMetaService.DataSources
    {
        get => DataSources?.ToList<IServiceDataSource>();
        set => DataSources = value as ICollection<ServiceDatabase>;
    }

    public ICollection<ServiceDatabase>? DataSources { get; set; }

    ICollection<IEntity>? IMetaService.Entities
    {
        get => Entities?.ToList<IEntity>();
        set => Entities = value as ICollection<Core.Models.Entity>;
    }
    public ICollection<Core.Models.Entity>? Entities { get; set; }

    ICollection<ILoader>? IMetaService.Loaders
    {
        get => Loaders?.ToList<ILoader>();
        set => Loaders = value as ICollection<Loader>;
    }
    public ICollection<Loader>? Loaders { get; set; }

    ICollection<IApi>? IMetaService.Apis
    {
        get => Apis?.ToList<IApi>();
        set => Apis = value as ICollection<Api.Api>;
    }
    public ICollection<Api.Api>? Apis { get; set; }

    public void Validate()
    {
        var validator = new MetaServiceValidator();
        validator.ValidateAndThrow(this);
    }
    
    public void Configure()
    {
        Entities = SortEntitiesByDependency();
        Loaders = SortLoadersByEntitySortOrder();
    }

    private ICollection<Loader> SortLoadersByEntitySortOrder()
    {
        var entities = Entities!.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        return Loaders!.OrderBy(l => entities[l.Target!.Entity].SortOrder).ToList();
    }


    private ICollection<Core.Models.Entity> SortEntitiesByDependency()
    {
        var entities = Entities!.ToList();

        foreach (var entity in entities)
        {
            foreach (var parent in entity.RelatedParents)
            {
                entities
                    .First(x => x.Name.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .RelatedChildren
                    .Add(entity.Name);
            }
        }

        // Rough sort entities by parent count

        entities.Sort((entity1, entity2) =>
            entity1.RelatedParents.Count.CompareTo(entity2.RelatedParents.Count));

        // hierarchy sort to place entities in dependency order

        var i = 0;
        var sortedEntities = new List<Core.Models.Entity>();
        while (entities.Count > 0)
        {
            var count = CountParentsInSortedEntities(entities, sortedEntities, i);

            if (count == entities[i].RelatedParents.Count)
            {
                sortedEntities.Add(entities[i]);
                entities.RemoveAt(i);
                i = 0;
            }
            else
            {
                if (++i >= entities.Count)
                {
                    i = 0;
                }
            }
        }

        i = 1;
        sortedEntities.ForEach(e => e.SortOrder = i++);

        return sortedEntities;

    }

    private static int CountParentsInSortedEntities(
            IList<Core.Models.Entity> unsortedEntities,
            IList<Core.Models.Entity> sortedEntities,
            int iteration)
    {
        var result = 0;

        foreach (string p in unsortedEntities[iteration].RelatedParents)
        {
            result += sortedEntities.Count(x => x.Name.Equals(p));
        }

        return result;
    }

}






