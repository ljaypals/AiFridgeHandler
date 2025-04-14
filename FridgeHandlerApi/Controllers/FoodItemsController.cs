using Microsoft.AspNetCore.Mvc;
using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.AspNetCore.Authorization;

namespace FridgeHandlerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] todo fix
    public class FoodItemsController : ControllerBase
    {
        private readonly IFoodItemService _foodItemService;

        public FoodItemsController(IFoodItemService foodItemService)
        {
            _foodItemService = foodItemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems()
        {
            return Ok(await _foodItemService.GetAllItemsAsync());
        }
        
        [HttpPost]
        public async Task<ActionResult<FoodItem>> AddFoodItem([FromBody] FoodItem item)
        {
            
            var authHeader = Request.Headers["Authorization"].ToString();
            Console.WriteLine("Authorization Header: " + authHeader);
            Console.WriteLine("Authenticated: " + User.Identity?.IsAuthenticated);
            Console.WriteLine("Email Claim: " + User.Claims.FirstOrDefault(c => c.Type == "email")?.Value);

            if (item == null) return BadRequest("Item is null");

            var createdItem = await _foodItemService.AddItemAsync(item);
            return CreatedAtAction(nameof(GetFoodItems), new { id = createdItem.Id }, createdItem);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] FoodItem updatedItem)
        {
            var updated = await _foodItemService.UpdateItemAsync(id, updatedItem);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _foodItemService.GetItemByIdAsync(id);
            if (item == null) return NotFound();

            await _foodItemService.DeleteItemAsync(id);
            return NoContent(); // 204 No Content
        }



    }
}