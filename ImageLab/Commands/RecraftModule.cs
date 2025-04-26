using Ideogram;
using ImageLab.Extension;
using ImageLab.Util;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Recraft;
using GenerateImageRequest = Recraft.GenerateImageRequest;

namespace ImageLab.Commands;

[SlashCommand("recraft", "Recraft commands")]
public class RecraftModule(RecraftClient recraftClient, HttpClient httpClient, ILogger<OpenAIModule> logger)  : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create an image")]
    public async Task CreateCommand(
        string prompt,
        int n = 1,
        ImageStyle style = ImageStyle.DigitalIllustration,
        ImageSize size = ImageSize.x1024x1024,
        string? negativePrompt = null,
        int? artisticLevel = null)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var imageClient = recraftClient.Image;

        var imageRequest = new GenerateImageRequest()
        {
            Prompt = prompt,
            N = n,
            Style = style,
            Size = size,
            ResponseFormat = ResponseFormat.B64Json
        };

        if (negativePrompt != null)
        {
            imageRequest.AdditionalProperties = new Dictionary<string, object> { { "negative_prompt", negativePrompt } };
        }

        if (artisticLevel != null)
        {
            imageRequest.Controls = new() {AdditionalProperties = new Dictionary<string, object> { { "artistic_level", artisticLevel } }};
        }

        var memoryStreams = new List<MemoryStream>();
        
        try
        {
            var response = await imageClient.GenerateImageAsync(imageRequest);
            var b64Images = response.Data.Select(d => d.B64Json).WhereNotNull().ToArray();
            var message = MessageUtil.Base64ImagesToMessage(memoryStreams, b64Images, "webp");
            await FollowupAsync(message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Failed to generate Recraft image.");
            await FollowupAsync("Failed to generate Recraft image. Tell admin to check logs.");
        }
    }
}