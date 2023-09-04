using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure;
using WhatToCook.WebApp.DataTransferObject.Responses;

namespace WhatToCook.Application.Services
{
    public class RecipeServiceQuery
    {
        private readonly DatabaseContext _dbcontext;

        public RecipeServiceQuery(DatabaseContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<List<RecipeResponse>> GetRecipes()
        {
            List<Recipe> recipes = await _dbcontext.Recipes.ToListAsync(); //== //select * from Recipes
            List<RecipeResponse> recipesMappingResult = new(); //każdy z 10 przepisów przerobiony na typ RecipeResponse
            foreach (var recipe in recipes)
            {
                RecipeResponse recipeResponse = RecipeResponse.MapFrom(recipe);
                recipesMappingResult.Add(recipeResponse);
            }
            return recipesMappingResult;
        }

        public async Task<RecipeResponse?> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var recipe = await _dbcontext.Recipes.Include(r => r.Ingredients)
                .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
            if (recipe is null)
            {
                return null;
            }

            var recipeResponse = RecipeResponse.MapFrom(recipe);

            return recipeResponse;
        }
    }
}