// Program.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;

Console.WriteLine("Starting AI Video Generator...");

var scenarioFilePath = "scenario.json";
Console.WriteLine($"Checking for scenario file at: {scenarioFilePath}");
if (!File.Exists(scenarioFilePath))
{
    Console.WriteLine("Scenario file not found.");
    return;
}

Console.WriteLine("Reading scenario file...");
var scenarioJson = await File.ReadAllTextAsync(scenarioFilePath);
Console.WriteLine("Deserializing scenario...");
var scenario = JsonSerializer.Deserialize<Scenario>(scenarioJson)!;

var outputDir = Path.Combine("output", SanitizeFileName(scenario.Title));
Console.WriteLine($"Creating output directories at: {outputDir}");
Directory.CreateDirectory(outputDir);
Directory.CreateDirectory(Path.Combine(outputDir, "voice"));
Directory.CreateDirectory(Path.Combine(outputDir, "video"));

Console.WriteLine("Saving scenario copy to output directory...");
await File.WriteAllTextAsync(Path.Combine(outputDir, "scenario.json"), scenarioJson);

// Step 1: Generate background music
var musicPath = Path.Combine(outputDir, "music.mp3");
Console.WriteLine("Step 1: Generating background music...");
await GenerateBackgroundMusic(scenario.BackgroundMusic, musicPath);

// Step 2: Generate cover image
var coverPath = Path.Combine(outputDir, "cover.png");
Console.WriteLine("Step 2: Generating cover image...");
// await GenerateCoverImage(scenario.Cover, coverPath);

// Step 3: Process segments
var voiceLine = new StringBuilder();
Console.WriteLine("Step 3: Processing segments...");
for (int i = 0; i < scenario.Segments.Count; i++)
{
    var segment = scenario.Segments[i];
    if (segment?.Type == "stock")
    {
        var videoPath = Path.Combine(outputDir, "video", $"{i}.mp4");

        Console.WriteLine($"Processing segment {i}...");
        // await GenerateVideo(segment.Video, videoPath);
    }
    voiceLine.Append(segment.Script);
    voiceLine.Append(" ");
}

// Step 4: Generate voice
Console.WriteLine("Step 4: Generating voice...");
var voicePath = Path.Combine(outputDir, "voice", "voice.mp3");
await GenerateVoice(voiceLine.ToString(), voicePath);

Console.WriteLine("Generation complete.");

// Utilities and services
static string SanitizeFileName(string name)
{
    Console.WriteLine("Sanitizing file name...");
    return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
}

static async Task GenerateBackgroundMusic(string prompt, string outPath)
{
    Console.WriteLine("[AI] Generating background music with prompt: " + prompt);
    // Placeholder: Call your music generation API here
    await File.WriteAllTextAsync(outPath, "[music data]");
    Console.WriteLine("[AI] Background music saved to: " + outPath);
}

static async Task GenerateCoverImage(string prompt, string outPath)
{
    var generator = new CoverGenerator();
    await generator.GenerateCoverImage(prompt, outPath);
}

static async Task GenerateVoice(string text, string outPath)
{
    var generator = new VoiceGenerator(); // optionally pass a different voice ID
    await generator.GenerateVoiceAsync(text, outPath);
}

static async Task GenerateVideo(string prompt, string outPath)
{
    var downloader = new StockVideoDownloader();
    await downloader.DownloadFirstVideoAsync(prompt, outPath);
}


// Models
public class Scenario
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("video_title")]
    public string VideoTitle { get; set; }

    [JsonPropertyName("video_description")]
    public string VideoDescription { get; set; }

    [JsonPropertyName("background_music")]
    public string BackgroundMusic { get; set; }

    [JsonPropertyName("cover")]
    public string Cover { get; set; }

    [JsonPropertyName("segments")]
    public List<Segment> Segments { get; set; } = new();
}

public class Segment
{
    [JsonPropertyName("script")]
    public string Script { get; set; }

    [JsonPropertyName("video")]
    public string Video { get; set; }

        [JsonPropertyName("type")]
    public string Type { get; set; }
}
