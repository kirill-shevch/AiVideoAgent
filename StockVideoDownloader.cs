using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class StockVideoDownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public StockVideoDownloader()
    {
        _apiKey = Environment.GetEnvironmentVariable("PEXELS_API_KEY")
                  ?? throw new InvalidOperationException("PEXELS_API_KEY environment variable is not set.");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", _apiKey);
    }

    public async Task DownloadFirstVideoAsync(string prompt, string outPath)
    {
        Console.WriteLine($"[AI] Searching for video with prompt: {prompt}");

        var requestUri = $"https://api.pexels.com/videos/search?query={Uri.EscapeDataString(prompt)}&per_page=1&orientation=landscape&size=medium";

        var response = await _httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ERROR] Pexels API request failed: {response.StatusCode}");
            return;
        }

        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var root = jsonDoc.RootElement;

        if (root.TryGetProperty("videos", out JsonElement videos) && videos.GetArrayLength() > 0)
        {
            var video = videos[0];
            if (video.TryGetProperty("video_files", out JsonElement videoFiles) && videoFiles.GetArrayLength() > 0)
            {
                var videoFile = videoFiles[0];
                if (videoFile.TryGetProperty("link", out JsonElement linkElement))
                {
                    var videoUrl = linkElement.GetString();
                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        Console.WriteLine($"[AI] Downloading video from: {videoUrl}");

                        var videoResponse = await _httpClient.GetAsync(videoUrl);
                        if (videoResponse.IsSuccessStatusCode)
                        {
                            var videoData = await videoResponse.Content.ReadAsByteArrayAsync();
                            await File.WriteAllBytesAsync(outPath, videoData);
                            Console.WriteLine($"[AI] Video saved to: {outPath}");
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Failed to download video: {videoResponse.StatusCode}");
                            return;
                        }
                    }
                }
            }
        }

        Console.WriteLine("[ERROR] No video found for the given prompt.");
    }
}
