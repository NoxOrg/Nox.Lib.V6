using Microsoft.AspNetCore.Components;
using Nox.Solution;
using Nox.Ui.Data;

namespace Nox.Ui.Pages;

public partial class Ui : ComponentBase
{
    [Parameter]
    public string Entity { get; set; } = string.Empty;

    [Inject] NoxSolution? Solution { get; set; }
    [Inject] NoxDataService NoxDataService { get; set; } = null!;

    protected bool isLoading = false;

    protected Entity? entity;

    protected IEnumerable<dynamic> data = Enumerable.Empty<dynamic>();

    protected override async Task OnParametersSetAsync()
    {
        if (Solution?.Domain?.Entities != null )
        {
            isLoading = true;

            entity = Solution.Domain.Entities.FirstOrDefault( e => 
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
