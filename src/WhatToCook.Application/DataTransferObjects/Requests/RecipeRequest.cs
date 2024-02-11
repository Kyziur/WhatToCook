namespace WhatToCook.Application.DataTransferObjects.Requests;

public class RecipeRequest
{
    public int Id { get; set; }
    public string Image { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public string PreparationDescription { get; set; } = string.Empty;
    public string TimeToPrepare { get; set; } = string.Empty;
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
}