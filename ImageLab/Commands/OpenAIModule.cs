using System.Text;
using ImageLab.Services;
using ImageLab.Services.OpenAI;
using ImageLab.Util;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageLab.Commands;

[SlashCommand("openai", "OpenAI commands")]
public class OpenAIModule(OpenAIGenerator openai, HttpClient httpClient, ILogger<OpenAIModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "Create new images")]
    public async Task CreateCommand
    (
        string prompt,
        OpenAITransparency? transparency = null,
        int? numberOfImages = null,
        int? outputCompression = null,
        OpenAIOutputFormat? outputFormat = null,
        OpenAIQuality? quality = null,
        OpenAISize? size = null,
        OpenAIStyle? style = null,
        OpenAIModel model = OpenAIModel.GPTImage1
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var memoryStreams = new List<MemoryStream>();

        try
        {
            outputFormat ??= model switch
            {
                OpenAIModel.GPTImage1 => OpenAIOutputFormat.Webp,
                _ => OpenAIOutputFormat.Png
            };

            var base64Images = await openai.GenerateBase64ImagesAsync
            (
                model,
                prompt,
                outputFormat.Value,
                transparency,
                numberOfImages,
                outputCompression,
                quality,
                size,
                style
            );

            var extension = outputFormat switch
            {
                OpenAIOutputFormat.Png => "png",
                OpenAIOutputFormat.Jpeg => "jpg",
                OpenAIOutputFormat.Webp => "webp",
            };

            var message = MessageUtil.Base64ImagesToMessage(memoryStreams, base64Images, extension);
            await FollowupAsync(message);
        }
        catch (ArgumentException ex)
        {
            await FollowupAsync(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Internal error generating OpenAI image");
            await FollowupAsync("Failed to generate image. Tell admin to check logs");
        }
        finally
        {
            foreach (var stream in memoryStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [SubSlashCommand("edit", "Edit images")]
    public async Task EditCommand(
        string prompt,
        Attachment imageAttachment,
        Attachment? imageAttachment2 = null,
        Attachment? imageAttachment3 = null,
        Attachment? imageAttachment4 = null,
        OpenAIModel model = OpenAIModel.GPTImage1,
        int? n = null,
        OpenAIQuality? quality = null,
        OpenAISize? size = null)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var memoryStreams = new List<MemoryStream>();
        
        var attachments = new [] {imageAttachment, imageAttachment2, imageAttachment3, imageAttachment4}
            .Where(att => att != null)
            .Select(att => att!)
            .ToArray();

        var base64Attachments = await MessageUtil.AttachmentsToBase64Async(httpClient, attachments);
        
        var base64Images = await openai.EditBase64ImageAsync
        (
            model,
            prompt,
            base64Attachments,
            quality,
            size,
            n
        );

        try
        {
            var message = MessageUtil.Base64ImagesToMessage(memoryStreams, base64Images, "png");
            await FollowupAsync(message);
        }
        catch (ArgumentException ex)
        {
            await FollowupAsync(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Internal error generating OpenAI image");
            await FollowupAsync("Failed to generate image. Tell admin to check logs");
        }
        finally
        {
            foreach (var stream in memoryStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }
}