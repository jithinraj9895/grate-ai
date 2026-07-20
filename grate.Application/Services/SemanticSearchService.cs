using System.Net.Http.Json;

public class SemanticSearchService
{
    private readonly HttpClient _httpClient;

    public SemanticSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SearchResponseDto> SearchAsync(string query)
    {
        var res = await _httpClient.GetFromJsonAsync<SearchResponseDto>($"search?query={Uri.EscapeDataString(query)}");
        if (res == null)
        {
            throw new NullReferenceException();
        }
        return res;
    }
}