using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Domain;

public record Image(string Path);

public class Recipe
{
    public int Id { get; }
    public string Name { get; private set; }
    public string PreparationDescription { get; private set; }
    public string ShortDescription { get; private set; } = "";
    public string TimeToPrepare { get; private set; }
    public List<Ingredient> Ingredients { get; private set; } = [];
    public Statistics Statistics { get; private set; }
    public Image Image { get; private set; }
    public List<RecipePlanOfMeals> RecipePlanOfMeals { get; private set; } = [];
    public List<Tag> Tags { get; private set; } = [];

    public Recipe(string name, string description, string timeToPrepare, List<Ingredient> ingredients, Image image)
    {
        SetName(name);
        SetDescription(description);
        SetTimeToPrepare(timeToPrepare);
        Ingredients = ingredients;
        Statistics = new Statistics();
        SetImage(image);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private Recipe()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name cannot be null, empty, or whitespace");
        }

        Name = name;
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description cannot be null, empty, or whitespace");
        }

        PreparationDescription = description;
    }

    public void SetShortDescription(string shortDescription)
    {
        ShortDescription = shortDescription;
    }

    public void SetTimeToPrepare(string timeToPrepare)
    {
        if (string.IsNullOrWhiteSpace(timeToPrepare))
        {
            throw new DomainException("TimeToPrepare cannot be null, empty, or whitespace");
        }

        TimeToPrepare = timeToPrepare;
    }

    public void SetImage(Image image)
    {
        Image = image;
    }

    public void UpdateIngredients(List<string> newIngredients)
    {
        if (newIngredients == null)
        {
            throw new ArgumentNullException(nameof(newIngredients));
        }

        Ingredients.Clear();
        foreach (string ingredient in newIngredients)
        {
            Ingredients.Add(new Ingredient(ingredient));
        }
    }

    public void SetTags(IEnumerable<Tag> tags) => Tags = new(tags);
}
