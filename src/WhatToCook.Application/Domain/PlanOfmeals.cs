namespace WhatToCook.Application.Domain;

public class PlanOfMeals
{
    public string Name { get; set; }
    public int Id { get; private set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public User User { get; set; }
    public IEnumerable<Recipe> Recipes { get; set; }
}
