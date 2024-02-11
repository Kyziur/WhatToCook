namespace WhatToCook.Application.Domain;

public class RecipeTag
{
    public int TagId { get; set; }
    public required Tag Tag { get; set; }
    public int RecipeId { get; set; }
    public required Recipe Recipe { get; set; }
}