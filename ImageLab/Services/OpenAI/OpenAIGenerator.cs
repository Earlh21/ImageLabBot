using System.Net.Mime;
using System.Text.Json;
using ImageLab.Services.OpenAI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    private readonly HttpClient httpClient;
    
    public OpenAIGenerator(OpenAIKey key)
    {
        this.key = key;
        httpClient = new ();
    }
    
    public async Task<Image[]> GenerateImageAsync(
        OpenAIModel model,
        string prompt, 
        OpenAITransparency? backgroundTransparency,
        int? n,
        int? outputCompression,
        OpenAIOutputFormat? outputFormat,
        OpenAIQuality? quality,
        OpenAISize? size,
        OpenAIStyle? style,
        string? user = null
    )
    {
        if (backgroundTransparency == Transparent && model != GPTImage1)
        {
            throw new ArgumentException($"Background transparency is only supported for the {GPTImage1} model");
        }

        if (backgroundTransparency == Transparent && outputFormat is not (Png or Webp))
        {
            throw new ArgumentException($"{Transparent} background is only supported for {Png} and {Webp} output formats");
        }

        if (n is < 1 or > 10)
        {
            throw new ArgumentException("n must be between 1 and 10");
        }

        if (model == DALLE3 && n is not (1 or null))
        {
            throw new ArgumentException($"Model {DALLE3} can only generate one image at a time");
        }

        if (outputCompression is < 0 or > 100)
        {
            throw new ArgumentException("Output compression must be between 0 and 100");
        }
        
        if(outputCompression != null &&
           (model != GPTImage1 || outputFormat is not (Webp or Jpeg)))
        {
            throw new ArgumentException($"Output compression is only supported for the {GPTImage1} model with {Webp} or {Jpeg} output format");
        }

        if (outputFormat != null && model != GPTImage1)
        {
            throw new ArgumentException($"Output format is only supported for the {GPTImage1} model");
        }

        if (model == DALLE3 && quality == Low)
        {
            throw new ArgumentException($"Only {High} and {Medium} sizes are supported for the {DALLE3} model");
        }

        if (model == DALLE2 && quality != null)
        {
            throw new ArgumentException($"Quality is not supported for the {DALLE2} model");
        }

        if (model == GPTImage1 && size is not (Landscape or Portrait or Square or null))
        {
            throw new ArgumentException($"Only {Landscape}, {Portrait}, and {Square} sizes are supported for the {GPTImage1} model");
        }

        if (model == DALLE2 && size is not (Square or SmallSquare or MediumSquare or null))
        {
            throw new ArgumentException($"Only {Square}, {MediumSquare}, and {SmallSquare} sizes are supported for the {DALLE2} model");
        }

        if (model == DALLE3 && size is not (Square or Portrait or Landscape or null))
        {
            throw new ArgumentException($"Only {Square}, {Landscape}, and {Portrait} sizes are supported for the {DALLE3} model");
        }

        if (style != null && model != DALLE3)
        {
            throw new ArgumentException($"Style is only supported for the {DALLE3} model");
        }

        var requestBody = new CreateImageRequest()
        {
            Prompt = prompt,
            N = n,
            OutputCompression = outputCompression,
            User = user,
            
            Model = model switch
            {
                GPTImage1 => "gpt-image-1",
                DALLE3 => "dalle-3",
                DALLE2 => "dalle-2",
            },
            
            OutputFormat = outputFormat switch
            {
                Png => "png",
                Jpeg => "jpeg",
                Webp => "webp",
                null => null,
                _ => "auto"
            },
        };

        if (model == GPTImage1)
        {
            requestBody.Background = backgroundTransparency switch
            {
                Opaque => "opaque",
                Transparent => "transparent",
                null => null,
                _ => "auto"
            };
            
            requestBody.Quality = quality switch
            {
                High => "high",
                Medium => "medium",
                Low => "low",
                null => null,
                _ => "auto"
            };

            requestBody.Size = size switch
            {
                Square => "1024x1024",
                Landscape => "1536x1024",
                Portrait => "1024x1536",
                null => null,
                _ => "auto"
            };
        }

        if (model == DALLE2)
        {
            requestBody.Size = size switch
            {

                SmallSquare => "256x256",
                MediumSquare => "512x512",
                Square => "1024x1024",
                null => null,
                _ => "auto"
            };
        }

        if (model == DALLE3)
        {
            requestBody.Quality = quality switch
            {
                High => "hd",
                Medium => "standard",
                null => null,
                _ => "auto"
            };
            
            requestBody.Size = size switch
            {
                Square => "1024x1024",
                Landscape => "1792x1024",
                Portrait => "1024x1792",
                null => null,
                _ => "auto"
            };

            requestBody.Style = style switch
            {
                Vivid => "vivid",
                Natural => "natural",
                null => null,
                _ => "auto"
            };
        }
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody));
        request.Headers.Add("Authorization", $"Bearer {key.Key}");
        request.Headers.Add("Content-Type", "application/json");
        
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException($"Failed to generate image. Status code: {response.StatusCode}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            var imageGenerations = JsonSerializer.Deserialize<ImageGeneration[]>(responseContent);

            if (imageGenerations == null)
            {
                throw new ServiceException($"Failed to generate image. Response: {responseContent}");
            }

            return imageGenerations
                .SelectMany(imageGeneration => imageGeneration.ToImages())
                .ToArray();
        }
        catch (JsonException ex)
        {
            throw new ServiceException($"Failed to deserialize response. Response: {responseContent}", ex);
        }
    }
}