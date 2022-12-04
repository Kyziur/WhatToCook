using Microsoft.EntityFrameworkCore;

namespace WhatToCook.Application.Domain
{

    public static class DataSeeder
    {

        public static void Seed(this ModelBuilder modelBuilder)

        {
            var User1 = new User
            {

                Email = "Pajacyk@gmail.com",

            };
            modelBuilder.Entity<User>().HasData(
                new User
                {

                    Email = "Pajacyk@gmail.com",

                }
            );

            var Recipe1 = new Recipe
            {
                Name = "omlet",
                Ingredients = new List<Ingredient> { new Ingredient { Name = "ffff" }, new Ingredient { Name = "fasas" } }
            };
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Name = "Omlet",
                    Ingredients =
                    new List<Ingredient> { new Ingredient { Name = "ffff" }, new Ingredient { Name = "fasas" } }

                }
            );

            modelBuilder.Entity<Ingredient>().HasData(new Ingredient
            {
                Name = "cebula"
            });
            modelBuilder.Entity<Tag>().HasData(new Tag { Name = " Mexican" });
            modelBuilder.Entity<Rating>().HasData(new Rating { Score = 1, User = User1, Recipe = Recipe1 });
            modelBuilder.Entity<Favourite>().HasData(new Favourite(Recipe1, User1));
            modelBuilder.Entity<PlanOfMeals>().HasData(new PlanOfMeals { FromDate = new DateTime(2022, 12, 12), ToDate = new DateTime(2023, 01, 12), Recipe = Recipe1, User = User1 });
            modelBuilder.Entity<RecipeTag>().HasData(new RecipeTag { Recipe = Recipe1, Tag = new Tag { Name = "Mexican" } });
        }
    }
}
