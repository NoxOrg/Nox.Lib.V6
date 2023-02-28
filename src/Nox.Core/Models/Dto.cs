using System.Collections.ObjectModel;
using Humanizer;
using Nox.Core.Components;
using Nox.Core.Interfaces.Dto;

namespace Nox.Core.Models;

public class Dto: MetaBase, IDto
{
    public string Name { get; set; }
    
    public string PluralName { get; set; }
    
    public ICollection<DtoAttribute> Attributes { get; set; } = new Collection<DtoAttribute>();
    
    public bool ApplyDefaults()
    {
        if (string.IsNullOrWhiteSpace(PluralName))
            PluralName = Name.Pluralize();
        return true;
    }
}