import { Recipe } from 'src/app/recipes/Recipe';
import { containsIgnoreCase } from '../../utils/utils';
import { RecipeCard } from 'src/app/recipes/recipe-card/recipe-card.component';

export type RecipeFilter = Readonly<{
  phrase: string;
  selectedTags: string[];
  selectedIngredients: string[];
}>;

export function filter(recipes: RecipeCard[], filter: RecipeFilter) {
  
  const byPhrase = filterByPhrase(recipes, filter.phrase);
  const byTags = filterByTags(byPhrase, filter.selectedTags);
  const byIngredients = filterByIngredients(byTags, filter.selectedIngredients);
  return byIngredients;
}

function filterByPhrase(recipes: RecipeCard[], pharse: string) {
  if (pharse.length === 0) {
    return recipes;
  }

  return recipes.filter(recipe =>
    recipe.name.toLowerCase().includes(pharse.toLowerCase())
  );
}

function filterByTags(recipes: RecipeCard[], tags: string[]) {
  if (tags.length === 0) {
    return recipes;
  }

  return recipes.filter(recipe =>
    recipe.tags.some(recipeTag =>
      tags.some(
        selectedTag => recipeTag.toLowerCase() === selectedTag.toLowerCase()
      )
    )
  );
}

function filterByIngredients(recipes: RecipeCard[], ingredients: string[]) {
  if (ingredients.length === 0) {
    return recipes;
  }

  return recipes.filter(recipe =>
    recipe.tags.some(recipeTag =>
      ingredients.some(
        selectedIngredient =>
          recipeTag.toLowerCase() === selectedIngredient.toLowerCase()
      )
    )
  );
}
function hasOneofIngredients(
  ingredients: string[],
  selectedIngredients: string[]
) {
  return (
    selectedIngredients.length > 0 &&
    selectedIngredients.some(ingredient => ingredients.includes(ingredient))
  );
}
function hasOneOfTags(tags: string[], selectedTags: string[]) {
  return tags.length > 0 && tags.some(tag => selectedTags.includes(tag));
}
