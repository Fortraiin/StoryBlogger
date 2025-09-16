using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

public class LocationService
{
    private readonly string _azureMapsApiKey;

    public LocationService(IConfiguration config)
    {
        _azureMapsApiKey = config.GetSection("Location")["AzureMapsApiKey"];
    }

    public async Task<string> GetStreetNameAsync(string coordinates)
    {
        if (string.IsNullOrWhiteSpace(coordinates) || string.IsNullOrWhiteSpace(_azureMapsApiKey))
            return "";

        // coordinates format: "latitude,longitude"
        var url = $"http://dev.virtualearth.net/REST/v1/Locations/{coordinates}?o=json&key={_azureMapsApiKey}";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return "";

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var resourceSets = doc.RootElement.GetProperty("resourceSets");
        if (resourceSets.GetArrayLength() == 0)
            return "";

        var resources = resourceSets[0].GetProperty("resources");
        if (resources.GetArrayLength() == 0)
            return "";

        var address = resources[0].GetProperty("address");
        if (address.TryGetProperty("addressLine", out var street))
            return street.GetString();

        return "";
    }
}