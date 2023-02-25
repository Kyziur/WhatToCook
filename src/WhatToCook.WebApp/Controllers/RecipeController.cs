using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
namespace WhatToCook.WebApp.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RecipeController : ControllerBase
{
    
    private readonly ILogger<RecipeController> _logger;
    private DatabaseContext _dbcontext;
    public RecipeController(ILogger<RecipeController> logger, DatabaseContext dbcontext)
    {
        _logger = logger;
        _dbcontext = dbcontext;
    }

    [HttpGet]
    public async Task<List<Recipe>> Get()
    {
        var recipes= await _dbcontext.Recipes.ToListAsync();
        return recipes;
    } 
  
    public class RecipeRequest
    {
        public string Name { get; set; }
        public List<string> Ingredients { get; set; }
        public string PreperationDescription { get; set; }
        public string TimeToPrepare { get; set; }
    }

    [HttpPost]
    public ActionResult Post(RecipeRequest request)
    {

        var recipe = new Recipe()
        {
            Name = request.Name,
            Ingredients = request.Ingredients.Select(ingredient => new Ingredient { Name = ingredient }).ToList(),
            Description = request.PreperationDescription,
            TimeToPrepare = request.TimeToPrepare
        };
        this._dbcontext.Recipes.Add(recipe);
        this._dbcontext.SaveChanges();
        return Ok();
    }
}
