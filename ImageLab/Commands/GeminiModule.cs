using ImageLab.ImageGenerators.Gemini;
using ImageLab.ImageGenerators.Gemini.Enums;
using ImageLab.Util;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ImageLab.Commands;

[SlashCommand("gemini", "Gemini commands")]
public class GeminiModule(GeminiGenerator gemini, HttpClient httpClient, ILogger<GeminiModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create new images")]
    public async Task CreateCommand(
        string prompt,
        int? numberOfImages = 1,
        GeminiModel model = GeminiModel.Gemini3
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var memoryStreams = new List<MemoryStream>();
        try
        {
            var base64Images = await gemini.GenerateImageAsync(model, prompt, numberOfImages ?? 1);
            var message = MessageUtil.Base64ImagesToMessage(memoryStreams, base64Images, "png"); 
            await FollowupAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Gemini create command");
            await FollowupAsync($"Failed to generate image: {ex.Message}");
        }
        finally
        {
            foreach (var stream in memoryStreams) await stream.DisposeAsync();
        }
    }

    [SubSlashCommand("edit", "Edit images")]
    public async Task EditCommand(
        string prompt,
        Attachment image,
        GeminiModel model = GeminiModel.Gemini3,
        int? numberOfImages = 1
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        
        if (model != GeminiModel.Gemini3)
        {
            await FollowupAsync("Only Gemini 3 supports image editing.");
        }
        
        var memoryStreams = new List<MemoryStream>();
        
        try
        {
            // Download the image
            var imageBytes = await httpClient.GetByteArrayAsync(image.Url);

            var base64Images = await gemini.GenerateImageAsync(model, prompt, numberOfImages ?? 1, [imageBytes]);
            
            var message = MessageUtil.Base64ImagesToMessage(memoryStreams, base64Images, "png");
            await FollowupAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Gemini edit command");
            await FollowupAsync($"Failed to edit image: {ex.Message}");
        }
        finally
        {
            foreach (var stream in memoryStreams) await stream.DisposeAsync();
        }
    }
}
