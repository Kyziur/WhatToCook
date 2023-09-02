using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.WebApp.DataTransferObject.Requests;

namespace WhatToCook.Application.Services;

public class RecipeService
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IMealPlanningRepository _mealPlanningRepository;

    public RecipeService(IRecipesRepository recipesRepository, IMealPlanningRepository mealPlanningRepository)
    {
        _recipesRepository = recipesRepository;
        _mealPlanningRepository = mealPlanningRepository;
    }

    public string SaveImage(string base64Image, string imagesDirectory)
    {
        string imagePath = "";
        if (!string.IsNullOrEmpty(base64Image))
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            string fileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine(imagesDirectory, "Images", fileName);

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
        await _recipesRepository.Create(recipe);
        return recipe;
    }

    public async Task<Recipe> Update(UpdateRecipeRequest request, string imagesDirectory)
    {
        var recipe = await _recipesRepository.GetRecipeByName(request.Name);
        // Find the recipe in the database using the provided ID
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
        await _recipesRepository.Update(recipe);
        return recipe;
    }
}