using FridgeHandler.Data.Models;

namespace FridgeHandler.Services.Interface
{
    public interface IFoodItemService
    {
        Task<IEnumerable<FoodItem>> GetAllItemsAsync();
        Task<FoodItem> AddItemAsync(FoodItem item);
        Task<IEnumerable<FoodItem>> GetExpiringItemsAsync();
        Task<FoodItem?> UpdateItemAsync(int id, FoodItem updatedItem);
        Task<FoodItem?> GetItemByIdAsync(int id);
        Task DeleteItemAsync(int id);


    }
}