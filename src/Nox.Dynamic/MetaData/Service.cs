using ETLBoxOffice.LicenseManager;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData;

public sealed class Service : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = "https://we-key-Nox-02.vault.azure.net/";
    public ServiceDatabase Database { get; set; } = new();
    public ICollection<Entity> Entities { get; set; } = null!;
    public ICollection<Loader> Loaders { get; set; } = null!;
    public ICollection<Api> Apis { get; set; } = null!;

    public void Validate(IReadOnlyDictionary<string,string> configurationVariables)
    {
        var validationInfo = new ServiceValidationInfo(Name,configurationVariables);

        var validator = new ServiceValidator(validationInfo);

        validator.ValidateAndThrow(this);

        Entities = SortEntitiesByDependancy();

        Loaders = SortLoadersByEntitySortOrder();

        // for loaders 
        LicenseCheck.LicenseKey = configurationVariables["EtlBox:LicenseKey"];

    }

    private ICollection<Loader> SortLoadersByEntitySortOrder()
    {
        var entities = Entities.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        return Loaders.OrderBy(l => entities[l.Target.Entity].SortOrder).ToList();
    }


    private ICollection<Entity> SortEntitiesByDependancy()
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

        // heirachy sort to place entities in dependency order

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

internal class ServiceValidator : AbstractValidator<Service>
{
    public ServiceValidator(ServiceValidationInfo info)
    {
        RuleForEach(service => service.Entities)
            .SetValidator(new EntityValidator(info));

        RuleForEach(service => service.Loaders)
            .SetValidator(new LoaderValidator(info));

        RuleForEach(service => service.Apis)
            .SetValidator(new ApiValidator(info));

        RuleFor(service => service.Database)
            .SetValidator(new ServiceDatabaseValidator(info));

    }
}

internal class ServiceValidationInfo
{
    public string ServiceName { get; private set; }
    public IReadOnlyDictionary<string,string> ConfigurationVariables { get; private set; }

    public ServiceValidationInfo(string serviceName, IReadOnlyDictionary<string, string> configurationVariables)
    {
        ServiceName = serviceName;

        ConfigurationVariables = configurationVariables;
    }
}

