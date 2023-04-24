using Microsoft.AspNetCore.Components;
using MudBlazor;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Entity;
using Nox.Ui.Data;

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
    protected string searchFilter = string.Empty;

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
        searchFilter = fillterText;
        mainGrid?.ReloadServerData();
    }

    protected async Task<GridData<NoxDataRow>> LoadServerData(GridState<NoxDataRow> state)
    {
        var pageSize = state.PageSize;
        var pageNum = state.Page;
        var orderState = state.SortDefinitions.FirstOrDefault();
        var filterStates = state.FilterDefinitions;

        string? orderByParameter = null;
        bool? orderDescendingParameter = null;
        string? gridFilter = null;
        string? columnFilters = null;
        string? filterParameter;
        int skipParameter = pageNum * pageSize;
        int topParameter = pageSize;

        if (orderState != null)
        {
            orderByParameter = BuildOrder(orderState);
            orderDescendingParameter = orderState.Descending;
        }

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            gridFilter = BuildGridFilter(searchFilter);
        }

        if (filterStates.Any())
        {
            columnFilters = BuildColumnFilters(filterStates);
        }

        filterParameter = CombineGridAndColumnFilters(gridFilter, columnFilters);

        var result = await NoxDataService.Find(entity!,
            skip: skipParameter, top: topParameter,
            orderby: orderByParameter, desc: orderDescendingParameter,
            filter: filterParameter
        );

        return new GridData<NoxDataRow>()
        {
            Items = result?.Rows ?? Enumerable.Empty<NoxDataRow>(),
            TotalItems = result?.RowCount ?? 0
        };
    }

    private static string? CombineGridAndColumnFilters(string? gridFilter, string? columnFilters)
    {
        if (columnFilters != null && gridFilter != null)
            return $"({columnFilters}) and ({gridFilter})";

        else if (columnFilters != null)
            return columnFilters;

        else if (gridFilter != null)
            return  gridFilter;

        return null;
    }

    private string BuildOrder(SortDefinition<NoxDataRow> orderState)
    {
        var stringIndex = new string(orderState.SortBy.Where(c => char.IsDigit(c)).ToArray());
        var intIndex = int.Parse(stringIndex);
        return headers[intIndex];
    }

    private string BuildGridFilter(string filterString)
    {
        string searchFilter = string.Empty;

        var prefix = string.Empty;
        foreach (var h in entity!.Attributes.Where<Core.Models.EntityAttribute>(a => a.NetDataType().Name.Equals("String")))
        {
            searchFilter += $"{prefix}contains({h.Name},'{filterString.Trim()}')";

            if (prefix.Length == 0)
                prefix = " or ";
        }

        return searchFilter;
    }

    private static string BuildColumnFilters(ICollection<FilterDefinition<NoxDataRow>> filterStates)
    {
        string fieldsFilter = string.Empty;
        var prefix = string.Empty;

        foreach (var f in filterStates)
        {
            var field = f.Title;
            var op = f.Operator;
            var value = f.Value;

            var clause = op!.ToLower() switch
            {
                "contains" => $"{prefix}contains({field},'{value}')",
                "not contains" => $"{prefix}contains({field},'{value}') ne true",
                "equals" => $"{prefix}{field} eq '{value}'",
                "not equals" => $"{prefix}{field} ne '{value}'",
                "starts with" => $"{prefix}startswith({field},'{value}')",
                "ends with" => $"{prefix}endswith({field},'{value}')",
                "is empty" => $"{prefix}{field} eq ''",
                "is not empty" => $"{prefix}{field} ne ''",
                _ => string.Empty,
            }; ;

            fieldsFilter += clause;

            if (prefix.Length == 0)
                prefix = " and ";
        }

        return fieldsFilter;
    }


}

