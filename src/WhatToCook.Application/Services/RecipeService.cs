using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Services;

public class RecipeService
{
    private readonly ILogger logger;
    private readonly IRecipesRepository recipesRepository;
    private readonly ITagsRepository tagsRepository;

    public RecipeService(IRecipesRepository recipesRepository, ITagsRepository tagsRepository, ILogger<RecipeService> logger)
    {
        this.recipesRepository = recipesRepository;
        this.tagsRepository = tagsRepository;
        this.logger = logger;
    }

    private async Task<IEnumerable<Tag>> GetOrCreateTags(IEnumerable<string> tags)
    {
        var existingTags = tagsRepository.GetTagsByNames(tags.Distinct().ToArray());

        if (existingTags.Count() == tags.Count())
        {
            return existingTags;
        }

        var missingTags = tags.Select(x => x.ToLowerInvariant()).Except(existingTags.Select(x => x.Name.ToLowerInvariant()));
        var newTags = await tagsRepository.Create(missingTags);

        return newTags.Concat(existingTags);
    }

    public async Task<Recipe> Create(RecipeRequest request, string imagesDirectory)
    {
        string imagePath = "";
        if (request.Image.Length > 0)
        {
            var imageInfo = new ImageInfo(request.Image, Guid.NewGuid().ToString(), imagesDirectory);
            imagePath = await recipesRepository.SaveImage(imageInfo);
        }
        else
        {
            imagePath = $"Images/default_image.png";
        }

        var ingredients = request.Ingredients.Select(ingredient => new Ingredient(ingredient)).ToList();
        var recipe = new Recipe
        (
            name: request.Name,
            description: request.PreparationDescription,
            timeToPrepare: request.TimeToPrepare,
            ingredients: ingredients,
            statistics: new Statistics(),
            image: imagePath
            );

        var tags = await GetOrCreateTags(request.Tags);
        recipe.SetTags(tags);

        await recipesRepository.Create(recipe);
        return recipe;
    }

    public async Task Delete(int id) => await recipesRepository.Delete(id);

    public async Task<Recipe> Update(UpdateRecipeRequest request, string imagesDirectory)
    {
        Recipe? recipe = await recipesRepository.GetById(request.Id);

        if (recipe == null)
        {
            logger.LogError($"Attempted to update a recipe: {request.Name}");
            throw new NotFoundException($"Cannot update {request.Id}");
        }

        recipe.RemoveImage(imagesDirectory);
        if (request.Image.Length > 0)
        {
            var imageInfo = new ImageInfo(request.Image, Guid.NewGuid().ToString(), imagesDirectory);
            string imagePath = await recipesRepository.SaveImage(imageInfo);
            recipe.SetImage(imagePath);
        }

        recipe.SetName(request.Name);
        recipe.SetDescription(request.PreparationDescription);
        recipe.SetTimeToPrepare(request.TimeToPrepare);
        recipe.UpdateIngredients(request.Ingredients);

        var tags = await GetOrCreateTags(request.Tags);
        recipe.SetTags(tags);

        await recipesRepository.Update(recipe);
        return recipe;
    }
}