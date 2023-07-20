using Microsoft.AspNetCore.Mvc;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.DataTransferObjects.Responses;
using WhatToCook.Application.Infrastructure;
using WhatToCook.Application.Services;

namespace WhatToCook.WebApp.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MealPlanningController : ControllerBase
{
    private readonly ILogger<MealPlanningController> _logger;
    private DatabaseContext _dbcontext;
    private readonly MealPlanningServiceQuery _mealPlanningServiceQuery;
    private readonly MealPlanningService _mealPlanningService;

    public MealPlanningController(ILogger<MealPlanningController> logger, DatabaseContext dbcontext,
        MealPlanningServiceQuery mealPlanningServiceQuery, MealPlanningService mealPlanningService)
    {
        _logger = logger;
        _dbcontext = dbcontext;
        _mealPlanningServiceQuery = mealPlanningServiceQuery;
        _mealPlanningService = mealPlanningService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlanOfMealResponse>>> Get()
    {
        var getPlanOfMeals = await _mealPlanningServiceQuery.GetPlanOfMeals();
        return Ok(getPlanOfMeals);
    }

    [HttpPost]
    public async Task<ActionResult> Post(PlanOfMealRequest planOfMealRequest)
    {
        await _mealPlanningService.Create(planOfMealRequest);
        return Ok();
    }
}