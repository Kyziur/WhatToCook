using Microsoft.AspNetCore.Mvc;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Services;

namespace WhatToCook.WebApp.Controllers;

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
        var getPlanOfMeals = await _mealPlanningServiceQuery.GetPlanOfMeals();
        return Ok(getPlanOfMeals);
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