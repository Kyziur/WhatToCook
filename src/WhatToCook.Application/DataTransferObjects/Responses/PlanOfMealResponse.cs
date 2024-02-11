namespace WhatToCook.Application.DataTransferObjects.Responses;

public class MealPlanForDayResponse
{
    public DateTime Day { get; set; }
    public IEnumerable<int> RecipesIds { get; set; } = Enumerable.Empty<int>();
}

public class PlanOfMealResponse
{
    public DateTime FromDate { get; set; }
    public int Id { get; set; }
    public required string Name { get; set; }
    public IEnumerable<MealPlanForDayResponse> Recipes { get; set; } = Enumerable.Empty<MealPlanForDayResponse>();
    public DateTime ToDate { get; set; }
}

public record PlanOfMealResponses(IEnumerable<PlanOfMealResponse> MealPlans);