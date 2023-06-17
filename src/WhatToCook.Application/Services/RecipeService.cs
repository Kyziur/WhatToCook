using Azure.Core;
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

        public string SaveImage(string base64Image, string imagesDirectory)
        {
            string imagePath = "";
            if (!string.IsNullOrEmpty(base64Image))
            {
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                string fileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(imagesDirectory, "images", fileName);

                System.IO.File.WriteAllBytes(filePath, imageBytes);
                imagePath = $"Images/{fileName}";
            }
            return imagePath;
        }
        public async Task<Recipe> Create(RecipeRequest request, string imagesDirectory)
        {
            var imagePath = this.SaveImage(request.Image, imagesDirectory);
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
            return recipe;
        }

        public async Task<Recipe> Update(UpdateRecipeRequest request, string imagesDirectory)
        {
            // Find the recipe in the database using the provided ID
            var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == request.Id);
            if (recipe == null)
            {
                throw new Exception($"Cannot update {request.Id}");
            }
            var imagePath = this.SaveImage(request.Image, imagesDirectory);

            // Update the recipe properties
            recipe.Name = request.Name;
            recipe.Description = request.PreparationDescription;
            recipe.TimeToPrepare = request.TimeToPrepare;
            recipe.Image = imagePath;

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
