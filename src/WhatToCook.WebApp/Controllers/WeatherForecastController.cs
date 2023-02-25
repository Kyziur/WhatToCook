//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using WhatToCook.Application.Domain;
//using WhatToCook.Application.Infrastructure;

//namespace WhatToCook.WebApp.Controllers;

//[ApiController]
//[Route("api/v1/[controller]")]
//public class WeatherForecastController : ControllerBase
//{
//    private static readonly string[] Summaries = new[]
//    {
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//    private readonly ILogger<WeatherForecastController> _logger;
//    private DatabaseContext _dbcontext;
//    public WeatherForecastController(ILogger<WeatherForecastController> logger, DatabaseContext dbcontext)
//    {
//        _logger = logger;
//        _dbcontext = dbcontext;
//    }

//    [HttpGet]
//    public IEnumerable<WeatherForecast> Get()
//    {
//        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
//        {
//            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            TemperatureC = Random.Shared.Next(-20, 55),
//            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
//        })
//        .ToArray();
//    }

//    public IEnumerable<Recipe> Get()
//    {
//        return Enumerable.Range(1, 5).Select(index => new Recipe
//        {
//        });
//    }
//    public class RecipeRequest
//    {
//        public string Name { get; set; }
//        public List<string> Ingredients { get; set; }
//        public string PreperationDescription { get; set; }

//        public string TimeToPrepare { get; set; }
//    }

//    [HttpPost]
//    public ActionResult Post(RecipeRequest request)
//    {

//        var recipe = new Recipe()
//        {
//            Name = request.Name,
//            Ingredients = request.Ingredients.Select(ingredient => new Ingredient { Name = ingredient }).ToList(),
//            Description = request.PreperationDescription,
//            TimeToPrepare = request.TimeToPrepare
//        };
//        this._dbcontext.Recipes.Add(recipe);
//        this._dbcontext.SaveChanges();
//        return Ok();
//    }

//}