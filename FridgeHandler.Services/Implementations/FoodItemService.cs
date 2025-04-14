using FridgeHandler.Data;
using FridgeHandler.Data.Models;
using FridgeHandler.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FridgeHandler.Services.Implementations
{
    public class FoodItemService : IFoodItemService
    {
        private readonly AppDbContext _context;

        public FoodItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FoodItem>> GetAllItemsAsync()
        {
            return await _context.FoodItems.ToListAsync();
        }

        public async Task<FoodItem> AddItemAsync(FoodItem item)
        {
            _context.FoodItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<IEnumerable<FoodItem>> GetExpiringItemsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.FoodItems
                .Where(item => item.ExpiryDate <= now.AddDays(3))
                .ToListAsync();
        }

        public async Task<FoodItem?> UpdateItemAsync(int id, FoodItem updatedItem)
        {
            var existing = await _context.FoodItems.FindAsync(id);
            if (existing == null) return null;

            existing.Name = updatedItem.Name;
            existing.ExpiryDate = updatedItem.ExpiryDate;
            existing.NutritionInfo = updatedItem.NutritionInfo;
            existing.ImageUri = updatedItem.ImageUri;
            existing.Quantity = updatedItem.Quantity;

            await _context.SaveChangesAsync();
            return existing;
        }
        
        public async Task<FoodItem?> GetItemByIdAsync(int id)
        {
            return await _context.FoodItems.FindAsync(id);
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item != null)
            {
                _context.FoodItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }


    }
}