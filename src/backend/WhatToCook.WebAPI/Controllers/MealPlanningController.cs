using Microsoft.AspNetCore.Mvc;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Services;

namespace WhatToCook.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MealPlanningController : ControllerBase
{
    private readonly MealPlanningService _mealPlanningService;
    private readonly MealPlanningServiceQuery _mealPlanningServiceQuery;

    public MealPlanningController(MealPlanningServiceQuery mealPlanningServiceQuery,
        MealPlanningService mealPlanningService)
    {
        _mealPlanningServiceQuery = mealPlanningServiceQuery;
        _mealPlanningService = mealPlanningService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlanOfMealResponse>>> Get()
    {
        var getPlanOfMeals = await _mealPlanningServiceQuery.GetAll();
        return Ok(getPlanOfMeals);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<List<PlanOfMealResponse>>> GetByName(string name, CancellationToken token)
    {
        var result = await _mealPlanningServiceQuery.GetByName(name, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("GetShoppingList/{mealPlanId}")]
    public async Task<ActionResult> GetIngredientsForShoppingList(int mealPlanId)
    {
        var response = await _mealPlanningServiceQuery.GetIngredientsForMealPlanById(mealPlanId);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> Post(PlanOfMealRequest planOfMealRequest)
    {
        await _mealPlanningService.Create(planOfMealRequest);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> Put(UpdatePlanOfMealRequest planOfMealRequest)
    {
        await _mealPlanningService.Update(planOfMealRequest);
        return Ok();
    }
}