using Newtonsoft.Json;
using Nox.Core.Interfaces.Entity;
using System.Dynamic;
using System.Net.Http.Json;

namespace Nox.Ui.Data;

internal class NoxDataService
{
    private readonly HttpClient _client;

    public NoxDataService(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<dynamic>> Find(IEntity entity, int skip = 0, int top = 10)
    {
        var query = $"https://localhost:5001/api/{entity.PluralName}?skip={skip}&top={top}";

        var data = await _client.GetFromJsonAsync<OData>(query);

        return data!.Value;
    }
}

internal class OData
{
    [JsonProperty("odata.metadata")]
    public string Metadata { get; set; } = null!;
    public List<ExpandoObject> Value { get; set; } = null!;
}