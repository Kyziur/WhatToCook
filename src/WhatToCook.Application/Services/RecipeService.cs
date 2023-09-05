using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.WebApp.DataTransferObject.Requests;

namespace WhatToCook.Application.Services;

public class RecipeService
{
    private readonly IRecipesRepository _recipesRepository;

    public RecipeService(IRecipesRepository recipesRepository)
    {
        _recipesRepository = recipesRepository;
    }

    public async Task<Recipe> Create(RecipeRequest request, string imagesDirectory)
    {
        var imagePath = _recipesRepository.SaveImage(request.Image, imagesDirectory);
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

        var imagePath = _recipesRepository.SaveImage(request.Image, imagesDirectory);

        if (string.IsNullOrWhiteSpace(recipe.Name))
        {
            throw new Exception("Name cannot be null, empty, or whitespace");
        }
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
    public async Task Delete(int id)
    {
        await _recipesRepository.Delete(id);
    }
}