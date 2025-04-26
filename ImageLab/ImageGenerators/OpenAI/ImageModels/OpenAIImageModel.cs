using ImageLab.Services.OpenAI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ImageLab.Services.OpenAI;

public abstract class OpenAIImageModel
{
    public abstract CreateImageRequest BuildCreateRequest(
        string prompt,
        OpenAIOutputFormat outputFormat,
        OpenAITransparency? backgroundTransparency,
        int? n,
        int? outputCompression,
        OpenAIQuality? quality,
        OpenAISize? size,
        OpenAIStyle? style,
        string? user = null);

    public abstract EditImageRequest BuildEditRequest(
        string prompt,
        int? n,
        string[] imagesBase64,
        OpenAIQuality? quality,
        OpenAISize? size,
        string? user = null);
}