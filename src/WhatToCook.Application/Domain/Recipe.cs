using WhatToCook.Application.Exceptions;

namespace WhatToCook.Application.Domain;

public class Recipe
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string TimeToPrepare { get; private set; }
    public List<Ingredient> Ingredients { get; private set; } = [];
    public Statistics Statistics { get; private set; }
    public string Image { get; private set; }
    public List<RecipePlanOfMeals> RecipePlanOfMeals { get; private set; } = [];
    public List<Tag> Tags { get; private set; } = [];

    public Recipe(string name, string description, string timeToPrepare, List<Ingredient> ingredients, Statistics statistics, string image)
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

        Description = description;
    }

    public void SetTimeToPrepare(string timeToPrepare)
    {
        if (string.IsNullOrWhiteSpace(timeToPrepare))
        {
            throw new DomainException("TimeToPrepare cannot be null, empty, or whitespace");
        }

        TimeToPrepare = timeToPrepare;
    }

    public void SetImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new DomainException("Image path cannot be null, empty, or whitespace");
        }

        Image = imagePath;
    }

    public void RemoveImage(string imagesDirectory)
    {
        if (string.IsNullOrWhiteSpace(Image) || Image.Equals("default_image.png"))
        {
            return;
        }

        try
        {
            string fullPath = Path.Combine(imagesDirectory, Image);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception exception)
        {
            throw new DomainException($"Failed to delete the existing image: {exception.Message}", exception);
        }
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

    public void SetTags(IEnumerable<Tag> tags) => Tags = tags.ToList();
}