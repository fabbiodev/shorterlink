using System.Net.Http.Json;

namespace ShorterURL.API.Services;

// возвращает код страны (ISO-2) по IP через ip-api.com
public class GeoService(HttpClient client)
{
    private record IpResult(string? CountryCode);

    public async Task<string?> GetCountryAsync(string ip)
    {
        try
        {
            var r = await client.GetFromJsonAsync<IpResult>($"http://ip-api.com/json/{ip}?fields=countryCode");
            return r?.CountryCode;
        }
        catch
        {
            return null; // молча игнорируем сбои
        }
    }
}
