using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.JSInterop;
using MudBlazor;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;
using Nox.Ui.Data;
using System.Net.Http;
using static MassTransit.ValidationResultExtensions;

namespace Nox.Ui.Pages;

public partial class Ui : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter]
    public string Entity { get; set; } = string.Empty;

    [Inject] IProjectConfiguration? NoxService { get; set; }
    [Inject] NoxDataService NoxDataService { get; set; } = null!;

    protected bool isLoading = true;
    protected bool isNotFound = false;

    protected IEntity? entity;
    protected IEntity? entityWhilstLoading;

    protected string searchString = string.Empty;
    protected IList<NoxDataRow> dummyData = Enumerable.Range(0,10).Select( i => new NoxDataRow()).ToList();

    protected int columnCount = 0;
    protected IList<string> headers = null!;

    protected MudDataGrid<NoxDataRow>? mainGrid;
    protected string globalFilter = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (NoxService?.Entities != null)
        {
            entityWhilstLoading = null;
            entity = null;
            isNotFound = true;
            isLoading = true;

            entityWhilstLoading = NoxService.Entities.FirstOrDefault(e =>
                e.PluralName.Equals(Entity, StringComparison.OrdinalIgnoreCase)
            );

            if (entityWhilstLoading != null)
            {
                var result = await NoxDataService.Find(entityWhilstLoading, 0, 0);
                headers = result.Headers;
                columnCount = result.ColumnCount;
                isNotFound = false;
                entity = entityWhilstLoading;
                entityWhilstLoading = null;
            }

        }
    }

    protected void ApplyFilter(string fillterText)
    {
        globalFilter = fillterText;
        mainGrid?.ReloadServerData();
    }

    protected async Task<GridData<NoxDataRow>> LoadServerData(GridState<NoxDataRow> state)
    {
        var pageSize = state.PageSize;
        var pageNum = state.Page;
        var orderState = state.SortDefinitions.FirstOrDefault();

        string? orderBy = null;
        bool? orderDescending = null;
        string? filter = null;

        if (orderState != null)
        {
            var stringIndex = new string(orderState.SortBy.Where(c => char.IsDigit(c)).ToArray());
            var intIndex = int.Parse(stringIndex);
            orderBy = headers[intIndex];
            orderDescending = orderState.Descending;
        }

        if (!string.IsNullOrWhiteSpace(globalFilter))
        {
            filter = string.Empty;
            var prefix = string.Empty;
            foreach (var h in entity!.Attributes.Where(a => a.NetDataType().Name.Equals("String")))
            {
                filter += $"{prefix}contains({h.Name},'{globalFilter.Trim()}')";
                if (prefix.Length == 0)
                    prefix = " or ";
            }
        }

        var result = await NoxDataService.Find(entity!, 
            skip: pageNum * pageSize, top: pageSize, 
            orderby: orderBy, desc: orderDescending,
            filter: filter    
        );

        return new GridData<NoxDataRow>() { 
            Items = result?.Rows ?? Enumerable.Empty<NoxDataRow>(), 
            TotalItems = result?.RowCount ?? 0
        };
    }

}

