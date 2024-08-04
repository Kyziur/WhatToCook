namespace WhatToCook.Application.DataTransferObjects.Requests;

public class PlanOfMealForDay
{
    public DateTime Day { get; set; }
    public IEnumerable<int> RecipesIds { get; set; } = Enumerable.Empty<int>();
}

public class PlanOfMealRequest
{
    public int Id { get; set; }
    public DateTime FromDate { get; set; }
    public required string Name { get; set; }
    public IEnumerable<PlanOfMealForDay> Recipes { get; set; } = Enumerable.Empty<PlanOfMealForDay>();
    public DateTime ToDate { get; set; }    
}

















