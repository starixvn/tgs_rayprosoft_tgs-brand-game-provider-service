using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Brand.GameProvider.Client;

public class BrandGameProviderClient : IBrandGameProviderClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;

    public BrandGameProviderClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task ChangeStatusByScheduleAsync()
    {
        var requestContent = new StringContent(
            JsonSerializer.Serialize(new { }, JsonSerializerOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync(
                "api/status/change-by-schedule",
                requestContent)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
    }
}