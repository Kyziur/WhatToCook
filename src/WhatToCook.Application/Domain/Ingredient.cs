namespace WhatToCook.Application.Domain;

public class Ingredient
{
    public int Id { get; private set; }
    public string Name { get; set; }
    public Recipe Recipe { get; set; }

    public Ingredient(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Ingredient name cannot be empty or null.");
        }
        Name = name;
    }
    private Ingredient() { }
}
