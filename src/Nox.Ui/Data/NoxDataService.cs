using System.Dynamic;
using System.Net.Http.Json;
using Nox.Solution;

namespace Nox.Ui.Data;

internal class NoxDataService : INoxDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NoxDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<dynamic>> Find(Entity entity, int skip = 0, int top = 10)
    {
        var httpClient = _httpClientFactory.CreateClient("NoxUi");

        var query = $"{entity.PluralName}?skip={skip}&top={top}";

        var data = await httpClient.GetFromJsonAsync<OData>(query);

        return data!.Value;
    }
}

internal class OData
{
    public List<ExpandoObject> Value { get; set; } = null!;
}