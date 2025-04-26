using ImageLab.Services.OpenAI.Models;

namespace ImageLab.Services.OpenAI;

public class GPTImage1Model : OpenAIImageModel
{
    public override CreateImageRequest BuildCreateRequest(
        string prompt,
        OpenAIOutputFormat outputFormat,
        OpenAITransparency? backgroundTransparency,
        int? n,
        int? outputCompression,
        OpenAIQuality? quality,
        OpenAISize? size,
        OpenAIStyle? style,
        string? user = null)
    {
        if (outputCompression is < 0 or > 100)
        {
            throw new ArgumentException("Output compression must be between 0 and 100");
        }

        if (n is < 0 or > 10)
        {
            throw new ArgumentException("N must be between 0 and 10");
        }

        if (size is OpenAISize.MediumSquare or OpenAISize.SmallSquare)
        {
            throw new ArgumentException("Medium and small square sizes are not supported for GPTImage1.");
        }

        if (style != null)
        {
            throw new ArgumentException("Style is only supported on DALLE3.");
        }

        return new()
        {
            Model = "gpt-image-1",
            Prompt = prompt,

            Background = backgroundTransparency switch
            {
                OpenAITransparency.Transparent => "transparent",
                OpenAITransparency.Opaque => "opaque",
                _ => null
            },

            Moderation = "low",
            N = n,
            OutputCompression = outputCompression,

            OutputFormat = outputFormat switch
            {
                OpenAIOutputFormat.Jpeg => "jpeg",
                OpenAIOutputFormat.Png => "png",
                OpenAIOutputFormat.Webp => "webp",
                _ => null
            },

            Quality = quality switch
            {
                OpenAIQuality.High => "high",
                OpenAIQuality.Medium => "medium",
                OpenAIQuality.Low => "low",
                _ => null
            },
            
            ResponseFormat = null,

            Size = size switch
            {
                OpenAISize.Square => "1024x1024",
                OpenAISize.Landscape => "1536x1024",
                OpenAISize.Portrait => "1024x1536",
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
        if (imagesBase64.Length == 0)
            throw new ArgumentException("At least one source image is required.");

        if (size is OpenAISize.MediumSquare or OpenAISize.SmallSquare)
            throw new ArgumentException("Medium and small square sizes are not supported for GPTImage1.");

        if (n is < 1 or > 10)
            throw new ArgumentException("n must be between 1 and 10.");

        return new()
        {
            Prompt = prompt,
            Images = imagesBase64,
            Model = "gpt-image-1",

            Quality = quality switch
            {
                OpenAIQuality.High => "high",
                OpenAIQuality.Medium => "medium",
                OpenAIQuality.Low => "low",
                _ => null
            },
            
            ResponseFormat = null,
            
            N = n,

            Size = size switch
            {
                OpenAISize.Square => "1024x1024",
                OpenAISize.Landscape => "1536x1024",
                OpenAISize.Portrait => "1024x1536",
                _ => null
            },

            User = user
        };
    }
}