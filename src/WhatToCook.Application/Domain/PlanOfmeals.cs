using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Domain;

public class PlanOfMeals
{
    public string Name { get; set; }
    public int Id { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }
    public User User { get; set; } = new User() { Email = "mail123@gmail.com" };
    public IEnumerable<Recipe> Recipes { get; set; }

    public PlanOfMeals(string name, DateTime fromDate, DateTime toDate, IEnumerable<Recipe> recipes)
    {
        Name = name;
        SetDates(fromDate, toDate); 
        Recipes = recipes;
    }
    private PlanOfMeals() { }

    public void ValidateDates()
    {
        if (FromDate < DateTime.UtcNow || ToDate < DateTime.UtcNow)
        {
            throw new IncorrectDateException("Dates cannot be in the past.");
        }

        if (ToDate < FromDate)
        {
            throw new IncorrectDateException("ToDate cannot be lower than FromDate.");
        }
    }
    public void SetDates(DateTime fromDate, DateTime toDate)
    {
        if (FromDate < DateTime.UtcNow || ToDate < DateTime.UtcNow)
        {
            throw new IncorrectDateException("Dates cannot be in the past.");
        }

        if (ToDate < FromDate)
        {
            throw new IncorrectDateException("ToDate cannot be lower than FromDate.");
        }

        FromDate = fromDate;
        ToDate = toDate;
    }
}

