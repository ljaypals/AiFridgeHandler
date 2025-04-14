using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using FridgeHandler.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FridgeHandler.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> // Inherits IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public AppDbContext() { }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call base method to apply Identity configurations

            // Seed Roles
            string adminRoleId = Guid.NewGuid().ToString();
            string userRoleId = Guid.NewGuid().ToString();

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" }
            );

            // Seed an Admin User
            string adminUserId = Guid.NewGuid().ToString();
            var adminUser = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@fridgehandler.com",
                Email = "admin@fridgehandler.com",
                NormalizedUserName = "ADMIN@FRIDGEHANDLER.COM",
                NormalizedEmail = "ADMIN@FRIDGEHANDLER.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Hash the password
            var hasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "AdminPass123!");

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign Admin Role to Admin User
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId }
            );

            // Seed Food Items
            modelBuilder.Entity<FoodItem>().HasData(
                new FoodItem { Id = 1, Name = "Milk", ExpiryDate = DateTime.UtcNow.AddDays(5), NutritionInfo = "Calories: 100" , ImageUri = "null", Quantity = 3},
                new FoodItem { Id = 2, Name = "Eggs", ExpiryDate = DateTime.UtcNow.AddDays(7), NutritionInfo = "Protein: 6g", ImageUri = "null", Quantity = 12},
                new FoodItem { Id = 3, Name = "Bread", ExpiryDate = DateTime.UtcNow.AddDays(3), NutritionInfo = "Carbs: 50g", ImageUri = "null", Quantity = 1}
            );

            // Seed Recipes
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Id = 1,
                    Name = "Omelette",
                    Ingredients = new List<string> { "Eggs", "Milk", "Salt" },
                    Instructions = "Whisk and cook in a pan.",
                    Calories = 150,
                    Fat = 10,
                    Protein = 12,
                    Servings = 1,
                    VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
                },
                new Recipe
                {
                    Id = 2,
                    Name = "French Toast",
                    Ingredients = new List<string> { "Eggs", "Milk", "Bread" },
                    Instructions = "Dip bread in egg mixture and fry.",
                    Calories = 250,
                    Fat = 8,
                    Protein = 9,
                    Servings = 2,
                    VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
                }
            );


            // Configure Recipe Ingredients Conversion
            // modelBuilder.Entity<Recipe>()
            //     .Property(r => r.Ingredients)
            //     .HasConversion(
            //         v => string.Join(',', v),
            //         v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            //     .Metadata.SetValueComparer(new ValueComparer<List<string>>(
            //         (c1, c2) => c1.SequenceEqual(c2),
            //         c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //         c => c.ToList()));
        }
    }
}
