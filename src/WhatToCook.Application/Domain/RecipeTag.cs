namespace WhatToCook.Application.Domain;

public class RecipeTag
{
    public int Id { get; private set; }
    public Tag Tag { get; set; }
    public Recipe Recipe { get; set; }
}
