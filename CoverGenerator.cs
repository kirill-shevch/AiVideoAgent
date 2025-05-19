using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

public class CoverGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public CoverGenerator()
    {
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task GenerateCoverImage(string prompt, string outPath)
    {
        Console.WriteLine("[AI] Generating cover image with prompt: " + prompt);

        var requestBody = new
        {
            prompt = prompt,
            n = 1,
            size = "1024x1024",
            model = "gpt-image-1"
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/images/generations", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("[ERROR] OpenAI DALL·E generation failed: " + await response.Content.ReadAsStringAsync());
            return;
        }

        var json = await response.Content.ReadFromJsonAsync<DalleResponse>();

        if (json?.data?[0]?.b64_json == null)
        {
            Console.WriteLine("[ERROR] No image data returned from OpenAI.");
            return;
        }

        byte[] imageBytes = Convert.FromBase64String(json.data[0].b64_json);
        await File.WriteAllBytesAsync(outPath, imageBytes);

        Console.WriteLine("[AI] Cover image saved to: " + outPath);
    }

    private class DalleResponse
    {
        public List<DalleImage> data { get; set; }
    }

    private class DalleImage
    {
        [JsonPropertyName("b64_json")]
        public string b64_json { get; set; }
    }
}
