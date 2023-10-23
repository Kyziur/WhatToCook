using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    Task<Recipe?> GetRecipeByName(string name);
    List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids);
    Task Create(Recipe recipe);
    Task Update(Recipe recipe);
    Task<string> SaveImage(ImageInfo imageInfo);
    Task Delete(int id);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly DatabaseContext _dbContext;
    private readonly ILogger _logger;
    private readonly IFileSaver _fileSaver;
    public RecipesRepository(DatabaseContext dbContext, ILogger<RecipesRepository> logger, IFileSaver fileSaver)
    {
        _dbContext = dbContext;
        _logger = logger;
        _fileSaver = fileSaver;
    }

    public List<Recipe> GetRecipesByIdForMealPlan(IEnumerable<int> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        var recipes = _dbContext.Recipes.Where(recipe => uniqueIds.Contains(recipe.Id)).ToList();
        var existingRecipeIds = recipes.Select(r => r.Id).ToList();
        if (existingRecipeIds.Count != uniqueIds.Count)
        {
            var missingRecipeIds = uniqueIds.Except(existingRecipeIds);
            var errorMessage = $"Not all recipes exist in the database: {string.Join(", ", missingRecipeIds)}";

            _logger.LogError(errorMessage); 
            throw new Exception(errorMessage);
        }
        return recipes;
    }
    public async Task<Recipe?> GetRecipeByName(string name)
    {
        return await _dbContext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Name == name);
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

           await  _fileSaver.SaveAsync(filePath, imageBytes);

            imageFullPath = $"Images/{finalFileName}";
        }
        catch (Exception exception)
        {
            _logger.LogError($"An error occurred while saving the image. Error: {exception.Message}");
            throw;
        }
        return imageFullPath;
    }
    public async Task Create(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
        await _dbContext.SaveChangesAsync();

    }
    public async Task Update(Recipe recipe)
    {
        _dbContext.Recipes.Update(recipe);
        await _dbContext.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id);
        if (recipe != null)
        {
            _dbContext.Recipes.Remove(recipe);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new NotFoundException($"Recipe with ID '{id}' not found.");
        }
    }
}