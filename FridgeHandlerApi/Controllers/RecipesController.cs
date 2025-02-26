using FridgeHandler.Services;
using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FridgeHandlerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes([FromQuery] IEnumerable<string> ingredients)
        {
            var recipes = await _recipeService.GetRecipesByIngredientsAsync(ingredients);
            return Ok(recipes);
        }

        [HttpPost]
        public async Task<ActionResult<Recipe>> AddRecipe(Recipe recipe)
        {
            var newRecipe = await _recipeService.AddRecipeAsync(recipe);
            return CreatedAtAction(nameof(GetRecipes), new { id = newRecipe.Id }, newRecipe);
        }

        [HttpGet("recommend")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecommendedRecipesAsync(
            [FromQuery] IEnumerable<string> ingredients)
        {
            if (!ingredients.Any())
            {
                return BadRequest("Please provide at least one ingredient.");
            }

            var suggestedRecipes = await _recipeService.GetRecommendedRecipesAsync(ingredients);

            return suggestedRecipes.Any()
                ? Ok(suggestedRecipes)
                : StatusCode(500, "Error: Unable to generate recipe suggestions at this time.");
        }
    }
}