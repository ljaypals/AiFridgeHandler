namespace FridgeHandler.Data.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Ingredients { get; set; } = new(); // Stored as comma-separated values in DB
        public string Instructions { get; set; }
        public int Calories { get; set; }
        public int Fat { get; set; }
        public int Protein { get; set; }
        public int Servings { get; set; }
        public string VideoUrl { get; set; }  // this is going to be a youtube link
        public string Category { get; set; } = "Uncategorized";
        public bool UserMade { get; set; } = false;

    }
}