using ImageLab.Services;
using ImageLab.Services.OpenAI;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageLab.Commands;

[SlashCommand("openai", "OpenAI commands")]
public class OpenAIModule(OpenAIGenerator openAIGenerator) : ApplicationCommandModule<ApplicationCommandContext>
{
    public async Task CreateCommand
    (
        string prompt,
        OpenAITransparency? transparency = null,
        int? numberOfImages = null,
        int? outputCompression = null,
        OpenAIOutputFormat outputFormat = OpenAIOutputFormat.Webp,
        OpenAIQuality? quality = null,
        OpenAISize? size = null,
        OpenAIStyle? style = null,
        OpenAIModel model = OpenAIModel.GPTImage1
    )
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        try
        {
            var images = await openAIGenerator.GenerateImageAsync
            (
                model,
                prompt,
                transparency,
                numberOfImages,
                outputCompression,
                outputFormat,
                quality,
                size,
                style
            );

            var message = new InteractionMessageProperties();

            var extension = outputFormat switch
            {
                OpenAIOutputFormat.Png => "png",
                OpenAIOutputFormat.Jpeg => "jpg",
                OpenAIOutputFormat.Webp => "webp",
            };
            
            IImageEncoder encoder = outputFormat switch
            {
                OpenAIOutputFormat.Png => new PngEncoder(),
                OpenAIOutputFormat.Jpeg => new JpegEncoder(),
                OpenAIOutputFormat.Webp => new WebpEncoder()
            };

            message.Attachments = images
                .Select((image, i) =>
                {
                    var memoryStream = new MemoryStream();
                    image.Save(memoryStream, encoder);

                    return new AttachmentProperties($"{i}.{extension}", memoryStream);
                }).ToArray();
            
            await FollowupAsync(message);
        }
        catch (ArgumentException ex)
        {
        }
        catch (ServiceException ex)
        {
        }
    }
}