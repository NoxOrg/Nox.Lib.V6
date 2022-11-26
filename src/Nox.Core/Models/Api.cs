using System.Collections.ObjectModel;
using FluentValidation;
using Nox.Core.Components;

namespace Nox.Core.Models;

public sealed class Api : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<ApiRoute> Routes { get; set; } = new Collection<ApiRoute>();
}



