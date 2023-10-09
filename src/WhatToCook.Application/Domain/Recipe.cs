namespace WhatToCook.Application.Domain;

public class Recipe
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string TimeToPrepare { get; private set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public Statistics Statistics { get; set; }
    public string Image { get; private set; }
    public List<PlanOfMeals> PlansOfMeals { get; set; }
    public Recipe(string name, string description, string timeToPrepare, List<Ingredient> ingredients, Statistics statistics, string image, List<PlanOfMeals> plansOfMeals)
    {
        SetName(name);
        SetDescription(description);
        SetTimeToPrepare(timeToPrepare);
        Ingredients = ingredients ?? new List<Ingredient>();
        Statistics = statistics;
        SetImage(image);
        PlansOfMeals = plansOfMeals;
    }

    private Recipe() { }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("Name cannot be null, empty, or whitespace");
        }
        this.Name = name;
    }
    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new Exception("Description cannot be null, empty, or whitespace");
        }
        this.Description = description;
    }
    public void SetTimeToPrepare(string timeToPrepare)
    {
        if (string.IsNullOrWhiteSpace(timeToPrepare))
        {
            throw new Exception("TimeToPrepare cannot be null, empty, or whitespace");
        }
        this.TimeToPrepare = timeToPrepare;
    }
    public void SetImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new Exception("Image path cannot be null, empty, or whitespace");
        }
        this.Image = imagePath;
    }

    public async Task RemoveImage(string imagesDirectory)
    {
        if (string.IsNullOrWhiteSpace(this.Image))
        {
            return;
        }

        try
        {
            string fullPath = Path.Combine(imagesDirectory, this.Image);

            bool fileExists = await Task.Run(() => File.Exists(fullPath));
            if (fileExists)
            {
                await Task.Run(() => File.Delete(fullPath));
            }
            this.Image = null;
        }
        catch (Exception exception)
        {
            throw new Exception($"Failed to delete the existing image: {exception.Message}", exception);
        }
    }
    public void UpdateIngredients(List<string> newIngredients)
    {
        if (newIngredients == null)
        {
            throw new ArgumentNullException(nameof(newIngredients));
        }

        Ingredients.Clear();
        foreach (var ingredient in newIngredients)
        {
            Ingredients.Add(new Ingredient(ingredient));
        }
    }
}

public class Statistics
{
    public int Id { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
}