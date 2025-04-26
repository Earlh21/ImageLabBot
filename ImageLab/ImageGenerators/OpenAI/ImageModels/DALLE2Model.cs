using ImageLab.Services.OpenAI.Models;

namespace ImageLab.Services.OpenAI;

public class DALLE2Model : OpenAIImageModel
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
        if (outputFormat != OpenAIOutputFormat.Png)
        {
            throw new ArgumentException($"Only png is supported for DALLE3.");
        }

        if (backgroundTransparency == OpenAITransparency.Transparent)
        {
            throw new ArgumentException($"Transparency is only supported for GPTImage1.");
        }

        if (n is < 0 or > 10)
        {
            throw new ArgumentException("N must be between 0 and 10");
        }

        if (outputCompression != null)
        {
            throw new ArgumentException("Only GPTImage1 supports output compression.");
        }

        if (quality != null)
        {
            throw new ArgumentException("Quality is not supported for DALLE2.");
        }

        if (size is not (OpenAISize.SmallSquare or OpenAISize.MediumSquare or OpenAISize.Square or null))
        {
            throw new ArgumentException("Only small, medium and square sizes are supported for DALLE2.");
        }

        if (style != null)
        {
            throw new ArgumentException("Style is only supported on DALLE3.");
        }

        return new()
        {
            Model = "dall-e-2",
            Prompt = prompt,
            Background = null,
            Moderation = null,
            N = n,
            OutputCompression = null,
            OutputFormat = null,
            Quality = null,
            ResponseFormat = "b64_json",

            Size = size switch
            {
                OpenAISize.SmallSquare => "256x256",
                OpenAISize.MediumSquare => "512x512",
                OpenAISize.Square => "1024x1024",
                _ => null
            },

            Style = null,
            User = user
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
        if (imagesBase64.Length != 1)
            throw new ArgumentException("DALLE2 edits accept exactly one source image.");

        if (quality != null)
            throw new ArgumentException("Quality is not customisable for DALLE2.");

        if (size is not (OpenAISize.SmallSquare or OpenAISize.MediumSquare or OpenAISize.Square or null))
            throw new ArgumentException("DALLE2 only supports small, medium, and large square sizes.");

        return new()
        {
            Prompt = prompt,
            Images = imagesBase64,
            Model = "dall-e-2",
            Quality = null,
            ResponseFormat = "b64_json",
            N = n,
            
            Size = size switch
            {
                OpenAISize.SmallSquare => "256x256",
                OpenAISize.MediumSquare => "512x512",
                OpenAISize.Square => "1024x1024",
                _ => null
            },
            
            User = user
        };
    }
}