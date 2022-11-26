using FluentValidation;
using Nox.Core.Components;
using Nox.Core.Constants;

namespace Nox.Core.Models;

public sealed class MetaService : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EndpointProvider { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = KeyVault.DefaultKeyVaultUri;
    public ServiceDatabase Database { get; set; } = new();
    public ServiceMessageBus MessageBus { get; set; } = new();
    public ICollection<Entity> Entities { get; set; } = null!;
    public ICollection<Loader> Loaders { get; set; } = null!;
    public ICollection<Api> Apis { get; set; } = null!;

    public void Configure()
    {
        Entities = SortEntitiesByDependency();
        Loaders = SortLoadersByEntitySortOrder();
    }

    private ICollection<Loader> SortLoadersByEntitySortOrder()
    {
        var entities = Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        return Loaders.OrderBy(l => entities[l.Target.Entity].SortOrder).ToList();
    }


    private ICollection<Entity> SortEntitiesByDependency()
    {
        var entities = Entities.ToList();

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
        var sortedEntities = new List<Entity>();
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
            IList<Entity> unsortedEntities,
            IList<Entity> sortedEntities,
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






