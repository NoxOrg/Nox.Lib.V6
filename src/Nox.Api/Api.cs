using System.Collections.ObjectModel;
using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Api;

namespace Nox.Api;

public sealed class Api : MetaBase, IApi
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    ICollection<IApiRoute>? IApi.Routes
    {
        get => Routes?.ToList<IApiRoute>();
        set => Routes = value as ICollection<ApiRoute>;
    }
    
    public ICollection<ApiRoute>? Routes { get; set; } = new Collection<ApiRoute>();
}



