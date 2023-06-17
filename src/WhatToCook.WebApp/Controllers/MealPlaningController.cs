using Microsoft.AspNetCore.Mvc;
using WhatToCook.Application.Infrastructure;
using WhatToCook.Application.Services;

namespace WhatToCook.WebApp.Controllers
{
    public class MealPlaningController : ControllerBase
    {
        private readonly ILogger<MealPlaningController> _logger;
        private DatabaseContext _dbcontext;
        private readonly MealPlanningServiceQuery _mealPlanningServiceQuery;
        private readonly MealPlanningService _mealPlanningService;

        public RecipeController (ILogger<MealPlaningController> logger, DatabaseContext dbcontext, MealPlanningServiceQuery mealPlanningServiceQuery, MealPlanningService mealPlanningService)
        {
            _logger = logger;
            _dbcontext = dbcontext; 
            _mealPlanningServiceQuery = mealPlanningServiceQuery;
            _mealPlanningService = mealPlanningService;
        }
        [HttpGet]

        [HttpPost]

        public async Task<ActionResult> Post

        [HttpPut]
        
    }
}
