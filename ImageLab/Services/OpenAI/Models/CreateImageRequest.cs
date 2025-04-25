using System.Text.Json.Serialization;

namespace ImageLab.Services.OpenAI.Models;

public class CreateImageRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
    
    [JsonPropertyName("background")]
    public string? Background { get; set; }
    
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("moderation")]
    public string Moderation { get; set; } = "low";
    
    [JsonPropertyName("n")]
    public int? N { get; set; }
    
    [JsonPropertyName("output_compression")]
    public int? OutputCompression { get; set; }
    
    [JsonPropertyName("output_format")]
    public string? OutputFormat { get; set; }
    
    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; } = "b64_json";
    
    [JsonPropertyName("size")]
    public string? Size { get; set; }
    
    [JsonPropertyName("style")]
    public string? Style { get; set; }
    
    [JsonPropertyName("user")]
    public string? User { get; set; }
}