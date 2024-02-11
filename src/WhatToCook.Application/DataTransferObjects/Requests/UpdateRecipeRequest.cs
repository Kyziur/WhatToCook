namespace WhatToCook.Application.DataTransferObjects.Requests;

public class UpdateRecipeRequest
{
    public int Id { get; set; }
    public required string Image { get; set; }
    public List<string> Ingredients { get; set; } = [];
    public required string Name { get; set; }
    public required string PreparationDescription { get; set; }
    public required string TimeToPrepare { get; set; }
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
}

public record TagDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
}