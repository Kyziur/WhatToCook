namespace WhatToCook.Application.Domain;

public class Recipe
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; set; }
    public string TimeToPrepare { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public Statistics Statistics { get; set; }
    public string Image { get; set; }
    public List<PlanOfMeals> PlansOfMeals { get; set; }
    public Recipe(string name, string description, string timeToPrepare, List<Ingredient> ingredients, Statistics statistics, string image, List<PlanOfMeals> plansOfMeals)
    {
        Name = name;
        Description = description;
        TimeToPrepare = timeToPrepare;
        Ingredients = ingredients ?? new List<Ingredient>();
        Statistics = statistics;
        Image = image;
        PlansOfMeals = plansOfMeals;
    }

    protected Recipe() { }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("Name cannot be null, empty, or whitespace");
        }
        this.Name = name;
    }
    public void UpdateIngredients(List<string> newIngredients)
    {
        if (newIngredients == null)
        {
            throw new ArgumentNullException(nameof(newIngredients));
        }

        Ingredients.Clear();
        foreach (var ingredient in newIngredients)
        {
            Ingredients.Add(new Ingredient(ingredient));
        }
    }
}

public class Statistics
{
    public int Id { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
}