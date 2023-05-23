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
    private readonly RecipeService _recipeService;
    public RecipeController(ILogger<RecipeController> logger, DatabaseContext dbcontext,
        IWebHostEnvironment environment, RecipeServiceQuery recipeServiceQuery, RecipeService recipeService)
    {
        _logger = logger;
        _dbcontext = dbcontext;
        _environment = environment;
        _recipeServiceQuery = recipeServiceQuery;
        _recipeService = recipeService;

    }

    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> Get()
    {
        var getRecipes = await _recipeServiceQuery.GetRecipes();
        return Ok(getRecipes);
    }

    //TODO: Secure name uniqueness by check on create and adding index on that column

    [HttpGet("{name}")]
    public async Task<ActionResult<RecipeResponse>> GetByName(string name)
    {
        var getRecipe = await _recipeServiceQuery.GetByName(name);
        return getRecipe is null ? NotFound() : Ok(getRecipe);
    }
    [HttpPost]
    public ActionResult Post(RecipeRequest request, string imagesDirectory)
    {
        var createRecipe = _recipeService.Create(request, imagesDirectory);
        return Ok(createRecipe);
    }


    [HttpPut]
    public async Task<ActionResult> Put(UpdateRecipeRequest request)
    {
        var updateRecipe = await _recipeService.Update(request);
        return Ok(updateRecipe);
    }
}