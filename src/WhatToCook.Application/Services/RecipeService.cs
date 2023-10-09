using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
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
        var imageInfo = new ImageInfo(request.Image, Guid.NewGuid().ToString(), imagesDirectory);
        var imagePath = await _recipesRepository.SaveImage(imageInfo);
        var ingredients = request.Ingredients.Select(ingredient => new Ingredient(ingredient)).ToList();
        var recipe = new Recipe
            (
            name: request.Name,
            description: request.PreparationDescription,
            timeToPrepare: request.TimeToPrepare,
            ingredients: ingredients,
            statistics : new Statistics(),
            image: imagePath,
            plansOfMeals: new List<PlanOfMeals>()
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
            throw new NotFoundException($"Cannot update {request.Id}");
        }

         recipe.RemoveImage(imagesDirectory);
        var imageInfo = new ImageInfo(request.Image, Guid.NewGuid().ToString(), imagesDirectory);
        var imagePath = await _recipesRepository.SaveImage(imageInfo);

        recipe.SetImage(imagePath);
        recipe.SetName(request.Name);
        recipe.SetDescription(request.PreparationDescription);
        recipe.SetTimeToPrepare(request.TimeToPrepare);
        recipe.UpdateIngredients(request.Ingredients);
        await _recipesRepository.Update(recipe);
        return recipe;
    }

    public async Task Delete(int id)
    {
        await _recipesRepository.Delete(id);
    }
}