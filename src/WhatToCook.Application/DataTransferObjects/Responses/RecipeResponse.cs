using WhatToCook.Application.Domain;

namespace WhatToCook.WebApp.DataTransferObject.Responses
{
    public class RecipeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Ingredients { get; set; }
        public string PreparationDescription { get; set; }
        public string TimeToPrepare { get; set; }
        public string ImagePath { get; set; }

        public static RecipeResponse MapFrom(Recipe recipe)
        {
            return new RecipeResponse
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.Ingredients.Select(x => x.Name),
                PreparationDescription = recipe.Description,
                TimeToPrepare = recipe.TimeToPrepare,
                ImagePath = recipe.Image
            };
        }
    }
}

