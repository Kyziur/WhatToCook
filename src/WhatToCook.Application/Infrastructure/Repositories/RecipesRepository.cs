using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    Task Create(Recipe recipe);

    Task Delete(int id);

    Task<Recipe?> GetByName(string name);

    List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids);

    Task<string> SaveImage(ImageInfo imageInfo);

    Task Update(Recipe recipe);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly DatabaseContext _dbContext;
    private readonly IFileSaver _fileSaver;
    private readonly ILogger _logger;

    public RecipesRepository(DatabaseContext dbContext, ILogger<RecipesRepository> logger, IFileSaver fileSaver)
    {
        _dbContext = dbContext;
        _logger = logger;
        _fileSaver = fileSaver;
    }

    public async Task Create(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id) ?? throw new NotFoundException($"Recipe with ID '{id}' not found.");
    }

    public async Task<Recipe?> GetByName(string name)
    {
        return await _dbContext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Name == name);
    }

    public List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        var recipes = _dbContext.Recipes.Where(recipe => uniqueIds.Contains(recipe.Id)).ToList();
        if (recipes.Count != uniqueIds.Count)
        {
            var missingRecipeIds = uniqueIds.Except(recipes.Select(x => x.Id));
            var errorMessage = "Not all recipes exist in the database.";
            _logger.LogError(errorMessage + " Missing IDs: {MissingIDs}", missingRecipeIds);
            throw new Exception(errorMessage);
        }
        return recipes;
    }

    public async Task<string> SaveImage(ImageInfo imageInfo)
    {
        string imageFullPath;
        if (string.IsNullOrEmpty(imageInfo.Base64Image))
        {
            return "";
        }

        try
        {
            string finalFileName = $"{imageInfo.FileNameWithoutExtension}{imageInfo.FileExtension}";

            string filePath = Path.Combine(imageInfo.ImagesDirectory, "Images", finalFileName);
            byte[] imageBytes = imageInfo.GetImageBytes();

            await _fileSaver.SaveAsync(filePath, imageBytes);

            imageFullPath = $"Images/{finalFileName}";
        }
        catch (Exception exception)
        {
            _logger.LogError("An error occurred while saving the image. Error: {message}", exception.Message);
            throw;
        }
        return imageFullPath;
    }

    public async Task Update(Recipe recipe)
    {
        _dbContext.Recipes.Update(recipe);
        await _dbContext.SaveChangesAsync();
    }
}