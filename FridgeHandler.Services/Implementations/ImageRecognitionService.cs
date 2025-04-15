using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FridgeHandler.Services.Implementations
{
    public class ImageRecognitionService : IImageRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly ILogger<ImageRecognitionService> _logger;

        public ImageRecognitionService(IConfiguration configuration, ILogger<ImageRecognitionService> logger)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<FoodScanResult> AnalyzeImageAsync(IFormFile imageFile)
{
    using var ms = new MemoryStream();
    await imageFile.CopyToAsync(ms);
    var base64Image = Convert.ToBase64String(ms.ToArray());

    var request = new
    {
        model = "gpt-4-turbo",
        messages = new[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = "Identify the food and return ONLY this JSON object: { \"name\": string, \"calories\": int, \"protein\": int, \"fat\": int, \"carbs\": int }" },
                    new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = $"data:image/jpeg;base64,{base64Image}"
                        }
                    }
                }
            }
        },
        max_tokens = 500
    };

    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

    try
    {
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseText = await response.Content.ReadAsStringAsync();

        // Log the raw response for debugging
        _logger.LogInformation("Raw GPT response:\n" + responseText);

        var jsonDoc = JsonDocument.Parse(responseText);
        var contentText = jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        // Safe extraction of only the JSON part
        var startIndex = contentText.IndexOf('{');
        var endIndex = contentText.LastIndexOf('}');
        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            _logger.LogError("Failed to locate valid JSON in GPT response:\n" + contentText);
            throw new Exception("Invalid AI response format");
        }

        var jsonOnly = contentText.Substring(startIndex, endIndex - startIndex + 1);
        var resultJson = JsonDocument.Parse(jsonOnly).RootElement;

        // Check if essential fields exist in the response and log if any are missing
        if (!resultJson.TryGetProperty("name", out var name) ||
            !resultJson.TryGetProperty("calories", out var calories) ||
            !resultJson.TryGetProperty("protein", out var protein) ||
            !resultJson.TryGetProperty("fat", out var fat) ||
            !resultJson.TryGetProperty("carbs", out var carbs))
        {
            _logger.LogError("Missing fields in the AI response.");
            throw new Exception("Missing fields in the AI response.");
        }

        var result = new FoodScanResult
        {
            Name = name.GetString() ?? "Unknown",
            Calories = calories.GetInt32(),
            Protein = protein.GetInt32(),
            Fat = fat.GetInt32(),
            Carbs = carbs.GetInt32()
        };

        if (string.IsNullOrWhiteSpace(result.Name) || result.Name.ToLower() == "unknown")
        {
            _logger.LogWarning("AI could not confidently identify the food.");
            throw new Exception("Food could not be identified.");
        }

        return result;

    }
    catch (Exception ex)
    {
        _logger.LogError("Error calling OpenAI Vision: " + ex.Message);
        throw;
    }
}

    }
}
