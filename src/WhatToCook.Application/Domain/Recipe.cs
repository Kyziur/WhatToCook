namespace WhatToCook.Application.Domain;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TimeToPrepare { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public Statistics Statistics { get; set; }
    public string Image { get; set; }
    public IEnumerable<PlanOfMeals> PlansOfMeals { get; set; }
}

public class Statistics
{
    public int Id { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
}