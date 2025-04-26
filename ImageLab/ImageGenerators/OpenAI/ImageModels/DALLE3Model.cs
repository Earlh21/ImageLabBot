using ImageLab.Services.OpenAI.Models;

namespace ImageLab.Services.OpenAI;

public class DALLE3Model : OpenAIImageModel
{
    public override CreateImageRequest BuildCreateRequest(string prompt, OpenAIOutputFormat outputFormat,
        OpenAITransparency? backgroundTransparency, int? n, int? outputCompression, OpenAIQuality? quality,
        OpenAISize? size, OpenAIStyle? style, string? user = null)
    {
        if (outputFormat != OpenAIOutputFormat.Png)
        {
            throw new ArgumentException($"Only png is supported for DALLE3.");
        }

        if (backgroundTransparency == OpenAITransparency.Transparent)
        {
            throw new ArgumentException($"Transparency is only supported for GPTImage1.");
        }
        
        if (n != null && n != 1)
        {
            throw new ArgumentException("DALLE3 only supports generating one image at a time.");
        }

        if (outputCompression != null)
        {
            throw new ArgumentException("Only GPTImage1 supports output compression.");
        }

        if (quality == OpenAIQuality.Low)
        {
            throw new ArgumentException($"Low quality is only supported for GPTImage1.");
        }

        if (size is OpenAISize.MediumSquare or OpenAISize.SmallSquare)
        {
            throw new ArgumentException("Medium and small square sizes are not supported for DALLE3.");
        }

        return new()
        {
            Model = "dall-e-3",
            Prompt = prompt,
            Background = null,
            Moderation = null,
            N = 1,
            OutputCompression = null,
            OutputFormat = null,

            Quality = quality switch
            {
                OpenAIQuality.High => "hd",
                OpenAIQuality.Medium => "standard",
                _ => null
            },

            ResponseFormat = "b64_json",

            Size = size switch
            {
                OpenAISize.Square => "1024x1024",
                OpenAISize.Landscape => "1792x1024",
                OpenAISize.Portrait => "1024x1792",
                _ => null
            },

            Style = style switch
            {
                OpenAIStyle.Vivid => "vivid",
                OpenAIStyle.Natural => "natural",
                _ => null
            },

            User = user,
        };
    }

    public override EditImageRequest BuildEditRequest(
        string prompt,
        int? n,
        string[] imagesBase64,
        OpenAIQuality? quality,
        OpenAISize? size,
        string? user = null)
    {
        throw new ArgumentException("Image edits are only supported by gpt-image-1 and dall-e-2.");
    }
}