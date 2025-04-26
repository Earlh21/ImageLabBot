using System.Text;
using NetCord;
using NetCord.Rest;

namespace ImageLab.Util;

public class MessageUtil
{
    public static async Task<InteractionMessageProperties> ImageUrlsToMessageAsync(HttpClient httpClient, IEnumerable<string> imageUrls)
    {
        var attachments = await Task.WhenAll(
            imageUrls.Select(url => ImageUrlToAttachmentAsync(httpClient, url))
        );

        var props = new InteractionMessageProperties();
        props.AddAttachments(attachments);
        return props;
    }
    
    public static async Task<AttachmentProperties> ImageUrlToAttachmentAsync(HttpClient httpClient, string imageUrl)
    {
        var stream = await httpClient.GetStreamAsync(imageUrl);
        var path = new Uri(imageUrl).LocalPath;
        var fileName = Path.GetFileName(path);
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "image" + Path.GetExtension(path);
        }

        return new (fileName, stream);
    }
    
    public static InteractionMessageProperties Base64ImagesToMessage(List<MemoryStream> memoryStreams, IEnumerable<string> base64Images, string extension)
    {
        var attachments = base64Images
            .Select((base64, i) => Base64ImageToAttachment(memoryStreams, base64, $"{i}.{extension}"))
            .ToArray();
        
        return new()
        {
            Attachments = attachments
        };
    }

    public static AttachmentProperties Base64ImageToAttachment(List<MemoryStream> memoryStreams, string base64, string filename)
    {
        var byteArray = Encoding.UTF8.GetBytes(base64);
        var ms = new MemoryStream(byteArray);
        ms.Position = 0;

        memoryStreams.Add(ms);
        
        return new Base64AttachmentProperties(filename, ms);
    }
    
    public static async Task<string[]> AttachmentsToBase64Async(HttpClient httpClient, Attachment[] attachments)
    {
        var tasks = attachments.Select(async att =>
        {
            // Attachment.Url is the Discord-CDN link to the file �̀cite�͂turn2view0�́
            var bytes = await httpClient.GetByteArrayAsync(att.Url);
            return Convert.ToBase64String(bytes);
        });

        return await Task.WhenAll(tasks);
    }
}