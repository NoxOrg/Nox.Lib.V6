using Newtonsoft.Json;
using Nox.Core.Interfaces.Entity;
using System.Dynamic;
using System.Net.Http.Json;

namespace Nox.Ui.Data;

internal class NoxDataService : INoxDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NoxDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<dynamic>> Find(IEntity entity, int skip = 0, int top = 10)
    {
        var httpClient = _httpClientFactory.CreateClient("NoxUi");

        var query = $"{entity.PluralName}?skip={skip}&top={top}";

        var data = await httpClient.GetFromJsonAsync<OData>(query);

        return data!.Value;
    }
}

internal class OData
{
    [JsonProperty("odata.metadata")]
    public string Metadata { get; set; } = null!;
    public List<ExpandoObject> Value { get; set; } = null!;
}