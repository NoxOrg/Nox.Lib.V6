using Microsoft.AspNetCore.Components;
using Microsoft.OData.ModelBuilder;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;
using Nox.Ui.Data;

namespace Nox.Ui.Pages;

public partial class Ui : ComponentBase
{
    [Parameter]
    public string Entity { get; set; } = string.Empty;

    [Inject] IProjectConfiguration? NoxService { get; set; }
    [Inject] NoxDataService NoxDataService { get; set; } = null!;

    protected bool isLoading = false;

    protected IEntity? entity;

    protected IEnumerable<dynamic> data = Enumerable.Empty<dynamic>();

    protected override async Task OnParametersSetAsync()
    {
        if (NoxService?.Entities != null )
        {
            isLoading = true;

            entity = NoxService.Entities.FirstOrDefault( e => 
                e.PluralName.Equals(Entity,StringComparison.OrdinalIgnoreCase)
            );

            if(entity is not null)
            {
                data = await NoxDataService.Find(entity) ?? Enumerable.Empty<dynamic>();
            }
            else
            {
                data = Enumerable.Empty<dynamic>();
            }

            isLoading = false;
        }
    }


}
