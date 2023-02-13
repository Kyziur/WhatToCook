namespace WhatToCook.Application.Domain;

/// <summary>
/// 
/// </summary>
public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Recipe Recipe { get; set; }
}
