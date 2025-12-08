using System.Text;
using System.Text.Json;

namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.TestHelpers;

public static class JsonUtility
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T?> DeserializeAsync<T>(HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static StringContent Serialize<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}

