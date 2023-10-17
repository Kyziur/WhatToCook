﻿using Microsoft.AspNetCore.Mvc;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Services;

namespace WhatToCook.WebApp.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly RecipeServiceQuery _recipeServiceQuery;
    private readonly RecipeService _recipeService;

    public RecipeController(IWebHostEnvironment environment, RecipeServiceQuery recipeServiceQuery, RecipeService recipeService)
    {
        _environment = environment;
        _recipeServiceQuery = recipeServiceQuery;
        _recipeService = recipeService;
    }

    private string GetBaseUrl()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return baseUrl;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> Get()
    {
        var getRecipes = await _recipeServiceQuery.GetRecipes();
        return Ok(getRecipes);
    }
    [HttpGet("{name}")]
    public async Task<ActionResult<RecipeResponse>> GetByName(string name)
    {
        var getRecipe = await _recipeServiceQuery.GetByName(name);
        if (getRecipe is null)
        {
            return NotFound();
        }

        getRecipe.ImagePath = $"{this.GetBaseUrl()}/{getRecipe.ImagePath}";
        return Ok(getRecipe);
    }

    [HttpPost]
    public async Task<ActionResult> Post(RecipeRequest request)
    {
        var filesDirectory = _environment.WebRootPath;
        await _recipeService.Create(request, filesDirectory);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> Put(UpdateRecipeRequest request)
    {
        var filesDirectory = _environment.WebRootPath;
        await _recipeService.Update(request, filesDirectory);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _recipeService.Delete(id);
        return NoContent();
    }
}