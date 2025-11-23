using Google.GenAI;
using Google.GenAI.Types; // Attempt to fix config type
using ImageLab.ImageGenerators.Gemini.Enums;
using ImageLab.Services;
using Microsoft.Extensions.Logging;

namespace ImageLab.ImageGenerators.Gemini;

public class GeminiGenerator(Client client, ILogger<GeminiGenerator> logger)
{
    private static string GetModelId(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini3 => "gemini-3-pro-image-preview",
            _ => "gemini-3-pro-image-preview"
        };
    }

    public async Task<string[]> GenerateImageAsync(
        GeminiModel model,
        string prompt,
        int numberOfImages)
    {
        return await GenerateImageAsync(model, prompt, numberOfImages, []);
    }

    public async Task<string[]> GenerateImageAsync(
        GeminiModel model,
        string prompt,
        int numberOfImages,
        IEnumerable<byte[]> imagesBytes)
    {
        string modelId = GetModelId(model);
        var base64Results = new List<string>();

        var config = new GenerateContentConfig
        {
            ResponseModalities = ["IMAGE"],
            CandidateCount = numberOfImages
        };

        var contents = new Content
        {
            Parts =
            [
                new() { Text = prompt }
            ]
        };

        contents.Parts.AddRange(imagesBytes.Select(bytes => new Part
            { InlineData = new() { MimeType = "image/png", Data = bytes } }));

        var response = await client.Models.GenerateContentAsync(modelId, contents, config);

        if (response.Candidates == null) return base64Results.ToArray();

        foreach (var candidate in response.Candidates)
        {
            var imageParts = candidate.Content.Parts.Where(p => p.InlineData != null);
            foreach (var part in imageParts)
            {
                string base64 = Convert.ToBase64String(part.InlineData.Data.ToArray());
                base64Results.Add(base64);
            }
        }

        return base64Results.ToArray();
    }
}