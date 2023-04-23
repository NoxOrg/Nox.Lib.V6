using Nox.Core.Interfaces.Entity;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Nox.Ui.Data;

internal class NoxDataService : INoxDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NoxDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<NoxDataGrid> Find(IEntity entity, int skip = 0, int top = 10, 
        string? orderby = null, bool? desc = null,
        string? filter = null)
    {
        var httpClient = _httpClientFactory.CreateClient("NoxUi");

        var descendingClause = desc ?? true ? " desc" : string.Empty;

        var orderByClause = string.IsNullOrEmpty(orderby) ? string.Empty : $"&$orderby={orderby}{descendingClause}";

        var filterClause = string.IsNullOrEmpty(filter) ? string.Empty : $"&$filter={filter}";

        var query = $"{entity.PluralName}?$skip={skip}&$top={top}&$count=true{orderByClause}{filterClause}";

        var response = await httpClient.GetAsync(query);

        if (!response.IsSuccessStatusCode)
        {
            string msg = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error loading data for {entity.Name} (Status: {response.StatusCode}) - {msg}");
        }

        var data = await response.Content.ReadFromJsonAsync<ODataResponse>()
            ?? throw new Exception($"Error loading data for {entity.Name}");
        
        var grid = new NoxDataGrid
        {
            Context = data.Context,
            Data = data.Value,
            Headers = entity.Attributes.Select(e => e.Name).ToList(),
            RowCount = data.Count
        };
        grid.ColumnCount = grid.Headers.Count;

        grid.Rows = new List<NoxDataRow>();
        foreach (var row in data.Value)
        {
            var dataRow = new NoxDataRow();
            grid.Rows.Add(dataRow);
            foreach(var header in grid.Headers) 
            {
                dataRow.Columns.Add(row[header].ToString() ?? string.Empty);
            }
        }

        return grid;

    }
}

public class NoxDataGrid
{
    public string Context = string.Empty;
    public int ColumnCount = 0;
    public int RowCount = 0;
    public IList<string> Headers = null!;
    public IList<NoxDataRow> Rows = null!;
    public IList<IDictionary<string, object>> Data = null!; 
}

public class NoxDataRow
{
    public IList<string> Columns = new List<string>();
}

internal class ODataResponse
{
    [JsonPropertyName("@odata.context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("@odata.count")]
    public int Count { get; set; } = 0;

    [JsonPropertyName("value")]
    public List<IDictionary<string, object>> Value { get; set; } = null!;
}