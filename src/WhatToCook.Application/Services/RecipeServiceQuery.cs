using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            List<Recipe> recipes = await _dbcontext.Recipes.ToListAsync(); //jest tu 10 przepisów
            List<RecipeResponse> recipesMappingResult = new(); //każdy z 10 przepisów przerobiony na typ RecipeResponse
            foreach (var recipe in recipes)
            {
                RecipeResponse recipeResponse = RecipeResponse.MapFrom(recipe);
                recipesMappingResult.Add(recipeResponse);
            }

            return recipesMappingResult;
        }
    }
}
