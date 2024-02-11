namespace WhatToCook.Application.Domain;

public class Tag
{
    public Tag(string name) => Name = name;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public IEnumerable<Recipe> Recipes { get; private set; }
}