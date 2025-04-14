using FridgeHandler.Data;
using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FridgeHandler.Services.Implementations
{
    public class RecipeService : IRecipeService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<RecipeService> _logger;
        private readonly IMemoryCache _cache;

        public RecipeService(AppDbContext context, IConfiguration configuration, ILogger<RecipeService> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _httpClient = new HttpClient();
            
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API key is missing in configuration.");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByIngredientsAsync(IEnumerable<string> ingredients)
        {
            return await _context.Recipes
                .Where(recipe => ingredients.All(ingredient => recipe.Ingredients.Contains(ingredient)))
                .ToListAsync();
        }

        public async Task<Recipe> AddRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<IEnumerable<Recipe>> GetRecommendedRecipesAsync(IEnumerable<string> ingredients)
        {
            string cacheKey = $"ai_recipes_{string.Join("_", ingredients)}";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Recipe> cachedRecipes))
            {
                _logger.LogInformation("Returning cached AI recipe recommendations.");
                return cachedRecipes;
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an AI fridge assistant suggesting structured recipes in JSON format." },
                    new { role = "user", content = $"Suggest three recipes using: {string.Join(", ", ingredients)}. " +
                                                   "Return a JSON array where each recipe includes: name, ingredients (list)," +
                                                   " instructions (string), calories (integer), fat (integer), protein (integer), " +
                                                   "servings (integer), and videoUrl (string). " +
                                                   "Try to find a relevant YouTube link for each recipe that shows how to make it, " +
                                                   "and include it as the videoUrl." }

                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to OpenAI for structured recipe suggestions...");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"OpenAI API request failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return Enumerable.Empty<Recipe>(); // Return an empty list instead of throwing an exception
            }

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response from OpenAI.");

            // Extract AI response
            var jsonDoc = JsonDocument.Parse(responseText);
            var recipeContent = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogInformation("Raw GPT content:\n" + recipeContent);

            // Clean the GPT formatting
            var cleanedJson = recipeContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            _logger.LogInformation("Cleaned GPT JSON:\n" + cleanedJson);

            // ✅ Use advanced JsonSerializerOptions
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            IEnumerable<Recipe>? recommendedRecipes = Enumerable.Empty<Recipe>();

            try
            {
                recommendedRecipes = JsonSerializer.Deserialize<IEnumerable<Recipe>>(cleanedJson, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse cleaned GPT response as JSON: " + ex.Message);
                return Enumerable.Empty<Recipe>();
            }


            // Prevent duplicates in the database
            foreach (var recipe in recommendedRecipes)
            {
                var exists = await _context.Recipes.AnyAsync(r => r.Name == recipe.Name);
                if (!exists)
                {
                    _context.Recipes.Add(recipe);
                }
            }

            await _context.SaveChangesAsync();

            // Cache the result for 24 hours
            _cache.Set(cacheKey, recommendedRecipes, TimeSpan.FromHours(24));

            return recommendedRecipes;
        }
    }
}
