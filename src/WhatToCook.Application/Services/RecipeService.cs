using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Services;

public class RecipeService
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly ILogger _logger;
    public RecipeService(IRecipesRepository recipesRepository, ILogger<RecipeService> logger)
    {
        _recipesRepository = recipesRepository;
        _logger = logger;
    }

    public async Task<Recipe> Create(RecipeRequest request, string imagesDirectory)
    {
        var imagePath = _recipesRepository.SaveImage(request.Image, imagesDirectory);
        var ingredients = request.Ingredients.Select(ingredient => new Ingredient(ingredient)).ToList();
        var recipe = new Recipe
            (
            name: request.Name,
            description: request.PreparationDescription,
            timeToPrepare: request.TimeToPrepare,
            ingredients: ingredients,
            statistics: null,
            image: imagePath,
            plansOfMeals: null
            );

        await _recipesRepository.Create(recipe);
        return recipe;
    }

    public async Task<Recipe> Update(UpdateRecipeRequest request, string imagesDirectory)
    {
        var recipe = await _recipesRepository.GetRecipeByName(request.Name);

        if (recipe == null)
        {
            _logger.LogError($"Attempted to update a recipe: {request.Name}");
            throw new Exception($"Cannot update {request.Id}");
        }


        if (!string.IsNullOrWhiteSpace(recipe.Image))
        {
            try
            {
                string fullPath = Path.Combine(imagesDirectory, recipe.Image);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to delete the existing image: {exception.Message}");
            }
        }

        var imagePath = _recipesRepository.SaveImage(request.Image, imagesDirectory);

        recipe.SetName(request.Name);
        recipe.Description = request.PreparationDescription;
        recipe.TimeToPrepare = request.TimeToPrepare;
        recipe.Image = imagePath;

        recipe.UpdateIngredients(request.Ingredients);
        await _recipesRepository.Update(recipe);
        return recipe;
    }

    public async Task Delete(int id)
    {
        await _recipesRepository.Delete(id);
    }
}