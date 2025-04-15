using FridgeHandler.Services;
using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FridgeHandlerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]

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
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetAllRecipes()
        {
            var allRecipes = await _recipeService.GetAllRecipesAsync();
            return Ok(allRecipes);
        }

        [HttpPost("add")]
        public async Task<ActionResult<Recipe>> AddRecipe(Recipe recipe)
        {
            var newRecipe = await _recipeService.AddRecipeAsync(recipe);
            return CreatedAtAction(nameof(GetRecipes), new { id = newRecipe.Id }, newRecipe);
        }

        [HttpPost("recommend")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecommendedRecipesAsync(
            [FromBody] IEnumerable<string> ingredients)
        {
            if (ingredients == null || !ingredients.Any())
            {
                return BadRequest("Please provide at least one ingredient.");
            }

            var suggestedRecipes = await _recipeService.GetRecommendedRecipesAsync(ingredients);

            return suggestedRecipes.Any()
                ? Ok(suggestedRecipes)
                : StatusCode(500, "Error: Unable to generate recipe suggestions at this time.");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound();

            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromBody] Recipe updatedRecipe)
        {
            if (id != updatedRecipe.Id)
                return BadRequest("Recipe ID mismatch.");

            var existing = await _recipeService.GetRecipeByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _recipeService.UpdateRecipeAsync(updatedRecipe);
            return NoContent(); // 204
        }


    }
}