namespace WhatToCook.Application.DataTransferObjects.Requests;

public class UpdateRecipeRequest
{
    public int Id { get; set; }
    public string Image { get; set; }
    public List<string> Ingredients { get; set; } = new List<string>();
    public string Name { get; set; }
    public string PreparationDescription { get; set; }
    public string TimeToPrepare { get; set; }
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
}

public record TagDto
{
    public int? Id { get; set; }
    public string Name { get; set; }
}