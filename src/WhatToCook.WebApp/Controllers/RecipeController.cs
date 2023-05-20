using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
using WhatToCook.Application.Services;
using WhatToCook.WebApp.DataTransferObject.Requests;
using WhatToCook.WebApp.DataTransferObject.Responses;

namespace WhatToCook.WebApp.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly ILogger<RecipeController> _logger;
    private DatabaseContext _dbcontext;
    private readonly IWebHostEnvironment _environment;
    private readonly RecipeServiceQuery _recipeServiceQuery;
    public RecipeController(ILogger<RecipeController> logger, DatabaseContext dbcontext,
        IWebHostEnvironment environment, RecipeServiceQuery recipeServiceQuery)
    {
        _logger = logger;
        _dbcontext = dbcontext;
        _environment = environment;
        _recipeServiceQuery= recipeServiceQuery;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> Get()
    {
        var getRecipe = await _recipeServiceQuery.GetRecipes();
        return Ok(getRecipe);
    }

    //TODO: Secure name uniqueness by check on create and adding index on that column

    [HttpGet("{name}")]
    public async Task<ActionResult<RecipeResponse>> GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return NotFound();
        }

        var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        if (recipe is null)
        {
            return NotFound();
        }

        var recipeResponse = RecipeResponse.MapFrom(recipe);

        return Ok(recipeResponse);
    }
    [HttpPost]
    public ActionResult Post(RecipeRequest request)
    {
        string ImagePath = "";
        if (!string.IsNullOrEmpty(request.Image))
        {
            byte[] imageBytes = Convert.FromBase64String(request.Image);
            string fileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

            System.IO.File.WriteAllBytes(filePath, imageBytes);
            ImagePath = $"/Images/{fileName}";
        }

        var recipe = new Recipe()
        {
            Name = request.Name,
            Ingredients = request.Ingredients.Select(ingredient => new Ingredient { Name = ingredient }).ToList(),
            Description = request.PreparationDescription,
            TimeToPrepare = request.TimeToPrepare,
            Image = ImagePath,
        };
        this._dbcontext.Recipes.Add(recipe);
        this._dbcontext.SaveChanges();
        return Ok();
    }


    [HttpPut]
    public async Task<ActionResult> Put([FromBody] UpdateRecipeRequest request)
    {
        // Find the recipe in the database using the provided ID
        var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == request.Id);
        if (recipe == null)
        {
            return NotFound();
        }

        // Update the recipe properties
        recipe.Name = request.Name;
        recipe.Description = request.PreparationDescription;
        recipe.TimeToPrepare = request.TimeToPrepare;
        recipe.Image = request.Image;


        // Clear the existing ingredients and add the updated ones
        recipe.Ingredients.Clear();
        foreach (var ingredient in request.Ingredients)
        {
            recipe.Ingredients.Add(new Ingredient { Name = ingredient });
        }


        // Save the changes to the database
        await _dbcontext.SaveChangesAsync();

        return Ok();
    }
}