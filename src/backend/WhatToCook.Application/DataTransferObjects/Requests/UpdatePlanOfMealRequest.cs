namespace WhatToCook.Application.DataTransferObjects.Requests;

public class UpdatePlanOfMealForDay
{
    public DateTime Day { get; set; }
    public IEnumerable<int> RecipeIds { get; set; } = Enumerable.Empty<int>();
}

public class UpdatePlanOfMealRequest
{
    public DateTime FromDate { get; set; }
    public int Id { get; set; }
    public required string Name { get; set; }
    public IEnumerable<PlanOfMealForDay> Recipes { get; set; } = Enumerable.Empty<PlanOfMealForDay>();
    public DateTime ToDate { get; set; }
}