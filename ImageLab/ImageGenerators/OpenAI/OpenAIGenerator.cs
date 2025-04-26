using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using ImageLab.Services.OpenAI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using static ImageLab.Services.OpenAI.OpenAIModel;
using static ImageLab.Services.OpenAI.OpenAIOutputFormat;
using static ImageLab.Services.OpenAI.OpenAIQuality;
using static ImageLab.Services.OpenAI.OpenAISize;
using static ImageLab.Services.OpenAI.OpenAIStyle;
using static ImageLab.Services.OpenAI.OpenAITransparency;

namespace ImageLab.Services.OpenAI;

public class OpenAIGenerator
{
    private readonly OpenAIKey key;
    private readonly ILogger logger;
    private readonly HttpClient httpClient;

    public OpenAIGenerator(OpenAIKey key, ILogger<OpenAIGenerator> logger, HttpClient httpClient)
    {
        this.key = key;
        this.logger = logger;
        this.httpClient = httpClient;
    }

    public async Task<string[]> GenerateBase64ImagesAsync(
        OpenAIModel model,
        string prompt,
        OpenAIOutputFormat outputFormat,
        OpenAITransparency? backgroundTransparency,
        int? n,
        int? outputCompression,
        OpenAIQuality? quality,
        OpenAISize? size,
        OpenAIStyle? style,
        string? user = null
    )
    {
        OpenAIImageModel modelObject = model switch
        {
            GPTImage1 => new GPTImage1Model(),
            DALLE3 => new DALLE3Model(),
            DALLE2 => new DALLE2Model(),
        };

        var requestBody = modelObject.BuildCreateRequest(prompt, outputFormat, backgroundTransparency, n,
            outputCompression, quality, size, style, user);
        var jsonContent = JsonSerializer.Serialize(requestBody);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations")
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Bearer {key.Key}");

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.Log(LogLevel.Error,
                $"Failed to generate OpenAI image.\n Status code: {response.StatusCode}.\nRequest body: {jsonContent}.\nResponse content: {responseContent}");
            throw new ImageGenException($"Failed to generate image. Status code: {response.StatusCode}");
        }

        return DeserializeImageGeneration(responseContent);
    }

    public async Task<string[]> EditBase64ImageAsync(
        OpenAIModel model,
        string prompt,
        string[] imagesBase64,
        OpenAIQuality? quality,
        OpenAISize? size,
        int? n,
        string? user = null)
    {
        OpenAIImageModel modelObject = model switch
        {
            GPTImage1 => new GPTImage1Model(),
            DALLE3 => new DALLE3Model(),
            DALLE2 => new DALLE2Model(),
            _ => throw new ArgumentOutOfRangeException(nameof(model))
        };

        var req = modelObject.BuildEditRequest(prompt, n, imagesBase64, quality, size, user);
        var form = BuildEditMultipart(req, imagesBase64);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/edits")
        {
            Content = form
        };
        request.Headers.Authorization = new("Bearer", key.Key);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.Log(LogLevel.Error, $"OpenAI edit failed ({response.StatusCode}): {responseContent}");
            throw new ImageGenException($"Failed to edit image. Status code: {response.StatusCode}");
        }

        return DeserializeImageGeneration(responseContent);
    }

    private static MultipartFormDataContent BuildEditMultipart(EditImageRequest req, string[] imagesBase64)
    {
        var form = new MultipartFormDataContent();

        for (var i = 0; i < imagesBase64.Length; i++)
        {
            var bytes = Convert.FromBase64String(imagesBase64[i]);
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png"); // default; adjust if needed
            form.Add(content, "image[]", $"image{i}.png");
        }

        form.Add(new StringContent(req.Prompt), "prompt");

        if (req.Model != null) form.Add(new StringContent(req.Model), "model");
        if (req.N != null) form.Add(new StringContent(req.N.Value.ToString()), "n");
        if (req.Size != null) form.Add(new StringContent(req.Size), "size");
        if (req.Quality != null) form.Add(new StringContent(req.Quality), "quality");
        if (req.User != null) form.Add(new StringContent(req.User), "user");

        return form;
    }

    private string[] DeserializeImageGeneration(string responseContent)
    {
        try
        {
            var generations = JsonSerializer.Deserialize<ImageGeneration[]>(responseContent);
            if (generations != null)
            {
                return generations
                    .SelectMany(g => g.Data)
                    .Select(d => d.B64Json)
                    .Where(b => b != null)
                    .Select(b => b!)
                    .ToArray();
            }
        }
        catch (JsonException)
        {
            try
            {
                var generation = JsonSerializer.Deserialize<ImageGeneration>(responseContent);
                if (generation != null)
                {
                    return generation.Data
                        .Select(d => d.B64Json)
                        .Where(b => b != null)
                        .Select(b => b!)
                        .ToArray();
                }
            }
            catch (JsonException ex)
            {
                logger.Log(LogLevel.Error, ex, "Failed to deserialize response.");
                throw new ImageGenException($"Failed to deserialize response.");
            }
        }

        throw new ImageGenException($"Failed to deserialize response.");
    }
}