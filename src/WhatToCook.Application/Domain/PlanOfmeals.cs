using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Domain;

public record RecipePerDay(DateTime Day, Recipe Recipe);
public class PlanOfMeals
{
    public string Name { get; set; }
    public int Id { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }
    public User User { get; set; } = new User() { Email = "mail123@gmail.com" };

    public List<RecipePlanOfMeals> RecipePlanOfMeals { get; set; } = new List<RecipePlanOfMeals> { };

    public PlanOfMeals(string name, DateTime fromDate, DateTime toDate, List<RecipePerDay> recipes)
    {
        Name = name;
        SetDates(fromDate, toDate);
        foreach (var recipe in recipes)
        {
            var recipePerDay = new RecipePlanOfMeals(recipe.Recipe, this, recipe.Day);
            RecipePlanOfMeals.Add(recipePerDay);
        }

    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PlanOfMeals() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    public void SetDates(DateTime fromDate, DateTime toDate)
    {
        if (fromDate.Date < DateTime.UtcNow.Date || toDate.Date < DateTime.UtcNow.Date)
        {
            throw new IncorrectDateException("Dates cannot be in the past.");
        }

        if (toDate.Date < fromDate.Date)
        {
            throw new IncorrectDateException("ToDate cannot be lower than FromDate.");
        }

        FromDate = fromDate;
        ToDate = toDate;
    }
}

