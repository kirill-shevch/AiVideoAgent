using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class VoiceGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _voiceId;

    public VoiceGenerator(string voiceId = "XB0fDUnXU5powFXDhCwa") // Default voice ID
    {
        _apiKey = Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")
                  ?? throw new InvalidOperationException("ELEVENLABS_API_KEY not set.");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

        _voiceId = voiceId;
    }

    public async Task GenerateVoiceAsync(string text, string outPath)
    {
        Console.WriteLine($"[AI] Generating voice for script: {text[..Math.Min(30, text.Length)]}...");

        var payload = new
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new
            {
                stability = 0.5,
                similarity_boost = 0.75
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"https://api.elevenlabs.io/v1/text-to-speech/{_voiceId}", payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ERROR] ElevenLabs TTS failed: {error}");
            return;
        }

        await using var audioStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(outPath);
        await audioStream.CopyToAsync(fileStream);

        Console.WriteLine("[AI] Voice clip saved to: " + outPath);
    }
}
