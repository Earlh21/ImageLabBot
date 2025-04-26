using System.Security.AccessControl;
using Ideogram;
using ImageLab.Extension;
using ImageLab.Util;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ImageLab.Commands;

[SlashCommand("ideogram", "Ideogram commands")]
public class IdeogramModule(IdeogramApi ideogram, HttpClient httpClient, ILogger<OpenAIModule> logger)  : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create an image")]
    public async Task CreateCommand(
        string prompt,
        bool magic = true,
        StyleType? style = null,
        AspectRatio aspectRatio = AspectRatio.ASPECT11,
        int numImages = 1)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        if (numImages is < 1 or > 8)
        {
            await FollowupAsync("Number of images must be between 1 and 8.");
            return;
        }
        
        var request = new ImageRequest
        {
            Prompt = prompt,
            Model = ModelEnum.V2,
            MagicPromptOption = magic ? MagicPromptOption.ON : MagicPromptOption.OFF,
            StyleType = style,
            AspectRatio = aspectRatio
        };

        try
        {
            var response = await ideogram.Generate.PostGenerateImageAsync(request);
            var imageUrls = response.Data.Select(d => d.Url).WhereNotNull().ToArray();
            var message = await MessageUtil.ImageUrlsToMessageAsync(httpClient, imageUrls);
            
            await FollowupAsync(message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Failed to generate Ideogram image.");
            await FollowupAsync("Failed to generate Ideogram image. Tell your admin to check logs.");
        }

    }
}