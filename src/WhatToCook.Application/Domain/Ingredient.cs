namespace WhatToCook.Application.Domain;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Recipe Recipe { get; set; }
}
