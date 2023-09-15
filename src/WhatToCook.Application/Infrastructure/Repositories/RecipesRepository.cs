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
    string SaveImage(string base64Image, string imagesDirectory);
    Task Delete(int id);
}

public class RecipesRepository : IRecipesRepository
{
    private readonly DatabaseContext _dbContext;
    private readonly ILogger _logger;
    public RecipesRepository(DatabaseContext dbContext, ILogger<RecipesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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
    public string SaveImage(string base64Image, string imagesDirectory)
    {
        string imagePath;
        if (string.IsNullOrEmpty(base64Image))
        {
            return "";
        }

        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            string fileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine(imagesDirectory, "Images", fileName);

            System.IO.File.WriteAllBytes(filePath, imageBytes);
            imagePath = $"Images/{fileName}";
        
        }
        catch (FormatException exception)
        {
            _logger.LogError($"Failed to convert the provided base64Image to bytes. Error: {exception.Message}");
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError($"An error occurred while saving the image. Error: {exception.Message}");
            throw;
        }
        return imagePath;
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