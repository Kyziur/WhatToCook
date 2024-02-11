using WhatToCook.Application.Domain;

namespace WhatToCook.Application.DataTransferObjects.Responses;

public class RecipeResponse
{
    public int Id { get; set; }
    public required string ImagePath { get; set; }
    public IEnumerable<string> Ingredients { get; set; } = Enumerable.Empty<string>();
    public required string Name { get; set; }
    public required string PreparationDescription { get; set; }
    public required string TimeToPrepare { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();

    public static RecipeResponse MapFrom(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Name = recipe.Name,
        Ingredients = recipe.Ingredients.Select(x => x.Name),
        PreparationDescription = recipe.Description,
        TimeToPrepare = recipe.TimeToPrepare,
        ImagePath = recipe.Image,
        Tags = recipe.Tags.Select(x => x.Name).ToArray()
    };
}