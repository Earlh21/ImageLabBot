using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace ImageLab.Services.OpenAI.Models;

public class ImageGeneration
{
    [JsonPropertyName("created")]
    public int Created { get; set; }
    
    [JsonPropertyName("data")]
    public ImageGenerationData[] Data { get; set; }
    
    [JsonPropertyName("usage")]
    public ImageGenerationUsage Usage { get; set; }

    public Image[] ToImages()
    {
        return Data
            .Select(data => data.ToImage())
            .Where(image => image != null)
            .Select(image => image!)
            .ToArray();
    }
}

public class ImageGenerationData
{
    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }
    
    [JsonPropertyName("revised_prompt")]
    public string? RevisedPrompt { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public Image? ToImage()
    {
        if (B64Json != null)
        {
            try
            {
                var bytes = Convert.FromBase64String(B64Json);
                return Image.Load(bytes);
            }
            catch (Exception e)
            {
                int d = 3;
            }
        }
        
        return null;
    }
}

public class ImageGenerationUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }
    
    [JsonPropertyName("input_token_details")]
    public InputTokenDetails InputTokenDetails { get; set; }
    
    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class InputTokenDetails
{
    [JsonPropertyName("image_tokens")]
    public int ImageTokens { get; set; }
    
    [JsonPropertyName("text_tokens")]
    public int TextTokens { get; set; }
}