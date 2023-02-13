namespace WhatToCook.Application.Domain;

public class PlanOfMeals
{
    public int Id { get; private set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Recipe Recipe { get; set; }
    public User User { get; set; }
}