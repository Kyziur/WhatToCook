using WhatToCook.Application.Domain;

namespace WhatToCook.Application.DataTransferObjects.Responses;

public class RecipeResponse
{
    public int Id { get; set; }
    public string ImagePath { get; set; }
    public IEnumerable<string> Ingredients { get; set; } = Enumerable.Empty<string>();
    public string Name { get; set; }
    public string PreparationDescription { get; set; }
    public string TimeToPrepare { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();

    public static RecipeResponse MapFrom(Recipe recipe)
    {
        return new RecipeResponse
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
}