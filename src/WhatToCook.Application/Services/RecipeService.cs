using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Extensions;
using WhatToCook.Application.Infrastructure.Images;
using WhatToCook.Application.Infrastructure.Repositories;

namespace WhatToCook.Application.Services;

public class RecipeService
{
    private readonly ILogger logger;
    private readonly IImagesManager _imagesManager;
    private readonly IRecipesRepository recipesRepository;
    private readonly ITagsRepository tagsRepository;

    public RecipeService(IRecipesRepository recipesRepository, ITagsRepository tagsRepository, ILogger<RecipeService> logger, IImagesManager imagesManager)
    {
        this.recipesRepository = recipesRepository;
        this.tagsRepository = tagsRepository;
        this.logger = logger;
        _imagesManager = imagesManager;
    }

    public async Task<Recipe> Create(RecipeRequest request)
    {
        var imagePath = await _imagesManager.Save(new ImageInfo(request.Image, Guid.NewGuid().ToString()));
        var ingredients = request.Ingredients.Select(ingredient => new Ingredient(ingredient)).ToList();
        var recipe = new Recipe
        (
            name: request.Name,
            description: request.PreparationDescription,
            timeToPrepare: request.TimeToPrepare,
            ingredients: ingredients,
            image: new Image(imagePath)
            );

        IEnumerable<Tag> tags = await GetOrCreateTags(request.Tags);
        recipe.SetTags(tags);

        await recipesRepository.Create(recipe);
        return recipe;
    }

    public async Task Delete(int id) => await recipesRepository.Delete(id);

    public async Task<Recipe> Update(UpdateRecipeRequest request)
    {
        Recipe? recipe = await recipesRepository.GetById(request.Id);

        if (recipe == null)
        {
            logger.LogError("Attempted to update a recipe: {RecipeName}", request.Name);
            throw new NotFoundException($"Cannot update {request.Id}");
        }

        if (request.Image.IsNotEmpty())
        {
            var imagePath = await _imagesManager.Replace(new ImageInfo(request.Image, Guid.NewGuid().ToString()));
            recipe.SetImage(recipe.Image with { Path = imagePath });
        }

        recipe.SetName(request.Name);
        recipe.SetDescription(request.PreparationDescription);
        recipe.SetTimeToPrepare(request.TimeToPrepare);
        recipe.UpdateIngredients(request.Ingredients);

        IEnumerable<Tag> tags = await GetOrCreateTags(request.Tags);
        recipe.SetTags(tags);

        await recipesRepository.Update(recipe);
        return recipe;
    }

    private async Task<IEnumerable<Tag>> GetOrCreateTags(IEnumerable<string> tags)
    {

        List<Tag> existingTags = await tagsRepository.GetTagsByNames(tags.Distinct().ToArray());
        if (existingTags.Count == tags.Distinct().Count())
        {
            return existingTags;
        }

        IEnumerable<string> missingTags = tags
            .Select(x => x.ToLowerInvariant())
            .Except(existingTags.Select(x => x.Name.ToLowerInvariant()));

        IEnumerable<Tag> newTags = await tagsRepository.Create(missingTags);

        return newTags.Concat(existingTags);
    }
}