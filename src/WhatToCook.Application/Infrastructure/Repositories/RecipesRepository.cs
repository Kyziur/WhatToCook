using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface IRecipesRepository
{
    Task<Recipe> GetRecipeByName(string name);
    List<Recipe> GetByNames(IEnumerable<string> names);
    Task Create(Recipe recipe);
    Task Update(Recipe recipe);
    string SaveImage(ImageInfo imageInfo);
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

    public List<Recipe> GetByNames(IEnumerable<string> names)
    {
        var recipes = _dbContext.Recipes.Include(recipe => recipe.PlansOfMeals)
        .Where(recipe => names.Contains(recipe.Name)).ToList();
        var existingRecipeNames = recipes.Select(r => r.Name).ToList();
        if (!existingRecipeNames.OrderBy(n => n).SequenceEqual(names.OrderBy(n => n)))
        {
            var missingRecipeNames = existingRecipeNames.Except(names);
            var errorMessage = $"Recipes do not exist in the database: {string.Join(", ", missingRecipeNames)}";

            _logger.LogError(errorMessage); 
            throw new Exception(errorMessage);
        }
        return recipes;
    }
    public async Task<Recipe?> GetRecipeByName(string name)
    {
        return await _dbContext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Name == name);
    }
    public string SaveImage(ImageInfo imageInfo)
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

            _fileSaver.SaveAsync(filePath, imageBytes);

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
        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync();
    }
}