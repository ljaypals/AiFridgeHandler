using FridgeHandler.Data.Models;

namespace FridgeHandler.Services.Interface
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetRecipesByIngredientsAsync(IEnumerable<string> ingredients);
        Task<IEnumerable<Recipe>> GetAllRecipesAsync();
        Task<Recipe> AddRecipeAsync(Recipe recipe);
        Task DeleteRecipeAsync(int id);
        Task UpdateRecipeAsync(Recipe recipe);
        Task<Recipe?> GetRecipeByIdAsync(int id);

        Task<IEnumerable<Recipe>> GetRecommendedRecipesAsync(IEnumerable<string> availableIngredients);

    }
}