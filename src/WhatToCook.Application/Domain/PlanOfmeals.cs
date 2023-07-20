namespace WhatToCook.Application.Domain;

public class PlanOfMeals
{
    public string Name { get; set; }
    public int Id { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public User User { get; set; } = new User() { Email = "mail123@gmail.com"};
    public IEnumerable<Recipe> Recipes { get; set; }
}
