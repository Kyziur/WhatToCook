using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure;


public static class DataSeeder
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        var user1 = new User
        {
            Email = "Pajacyk@gmail.com",
        };
        modelBuilder.Entity<User>().HasData(user1);
        var recipe1 = new Recipe
        {
            Name = "omlet",
            Ingredients = new List<Ingredient> { new Ingredient { Name = "ffff" }, new Ingredient { Name = "fasas" } }
        };
        modelBuilder.Entity<Recipe>().HasData(recipe1);
        modelBuilder.Entity<Ingredient>().HasData(new Ingredient
        {
            Name = "cebula"
        });
        modelBuilder.Entity<Tag>().HasData(new Tag { Name = " Mexican" });
        modelBuilder.Entity<Rating>().HasData(new Rating { Score = 1, User = user1, Recipe = recipe1 });
        modelBuilder.Entity<Favourite>().HasData(new Favourite(recipe1, user1));
        modelBuilder.Entity<PlanOfMeals>().HasData(new PlanOfMeals { FromDate = new DateTime(2022, 12, 12), ToDate = new DateTime(2023, 01, 12), Recipe = recipe1, User = user1 });
        modelBuilder.Entity<RecipeTag>().HasData(new RecipeTag { Recipe = recipe1, Tag = new Tag { Name = "Mexican" } });
    }
}
