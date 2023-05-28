using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
using WhatToCook.WebApp.DataTransferObject.Requests;
using WhatToCook.WebApp.DataTransferObject.Responses;

namespace WhatToCook.Application.Services
{
    public class RecipeService
    {

        private DatabaseContext _dbcontext;


        public RecipeService(DatabaseContext dbcontext)
        {
            _dbcontext = dbcontext;

        }
        public async Task Create(RecipeRequest request, string imagesDirectory)
        {
            string imagePath = "";
            if (!string.IsNullOrEmpty(request.Image))
            {
                byte[] imageBytes = Convert.FromBase64String(request.Image);
                string fileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(imagesDirectory, "Images", fileName);

                System.IO.File.WriteAllBytes(filePath, imageBytes);
                imagePath = $"/Images/{fileName}";
            }

            var recipe = new Recipe()
            {
                Name = request.Name,
                Ingredients = request.Ingredients.Select(ingredient => new Ingredient { Name = ingredient }).ToList(),
                Description = request.PreparationDescription,
                TimeToPrepare = request.TimeToPrepare,
                Image = imagePath,
            };
            await this._dbcontext.Recipes.AddAsync(recipe);
            await this._dbcontext.SaveChangesAsync();
        }

        public async Task<Recipe> Update(UpdateRecipeRequest request)
        {
            // Find the recipe in the database using the provided ID
            var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == request.Id);
            if (recipe == null)
            {
                throw new Exception($"Cannot update {request.Id}");
            }

            // Update the recipe properties
            recipe.Name = request.Name;
            recipe.Description = request.PreparationDescription;
            recipe.TimeToPrepare = request.TimeToPrepare;
            recipe.Image = request.Image;


            // Clear the existing ingredients and add the updated ones
            recipe.Ingredients.Clear();
            foreach (var ingredient in request.Ingredients)
            {
                recipe.Ingredients.Add(new Ingredient { Name = ingredient });
            }


            // Save the changes to the database
            await _dbcontext.SaveChangesAsync();
            return recipe;
        }
    }
}
