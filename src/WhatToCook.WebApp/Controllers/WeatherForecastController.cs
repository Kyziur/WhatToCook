using Microsoft.AspNetCore.Mvc;

namespace WhatToCook.WebApp.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    public class RecipeRequest
    {
        public string Name { get; set; }
        public string Ingredients { get; set; }
        public string PreperationDescription { get; set; }
    }

    [HttpPost]
    public ActionResult Post(RecipeRequest request)
    {
        RecipeRequest recipeRequest = new()
        {
            Name = request.Name + "resp",
            Ingredients = request.Ingredients + "resp",
            PreperationDescription = request.PreperationDescription + "resp"

        };

        return Ok(recipeRequest);
    }
}