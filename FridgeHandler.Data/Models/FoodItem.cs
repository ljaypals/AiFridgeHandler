namespace FridgeHandler.Data.Models
{
    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string NutritionInfo { get; set; } // Optional: JSON or plain text
        public string? ImageUri { get; set; }
        public int Quantity { get; set; }
    }
}