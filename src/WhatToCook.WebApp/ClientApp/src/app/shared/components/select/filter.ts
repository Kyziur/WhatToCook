import { Recipe } from 'src/app/recipes/Recipe';
import { containsIgnoreCase } from '../../utils/utils';
import { RecipeCard } from 'src/app/recipes/recipe-card/recipe-card.component';

export type RecipeFilter = Readonly<{
  phrase: string;
  selectedTags: string[];
  selectedIngredients: string[];
}>;

export function filter(recipes: RecipeCard[], filter: RecipeFilter) {
  return filter.phrase.length === 0 &&
    filter.selectedTags.length === 0 &&
    filter.selectedIngredients.length === 0
    ? recipes
    : recipes.filter((recipe) => {
        console.error('recipe', {
          recipe: recipe,
          contains: containsIgnoreCase(recipe.name, filter.phrase),
          phrase: filter.phrase,
        });
        return (
          containsIgnoreCase(recipe.name, filter.phrase) ||
          hasOneOfTags(recipe.tags, filter.selectedTags) ||
          hasOneofIngredients(recipe.ingredients, filter.selectedIngredients)
        );
      });
}

function hasOneofIngredients(
  ingredients: string[],
  selectedIngredients: string[]
) {
  return (
    selectedIngredients.length > 0 &&
    selectedIngredients.some((ingredient) => ingredients.includes(ingredient))
  );
}
function hasOneOfTags(tags: string[], selectedTags: string[]) {
  return tags.length > 0 && tags.some((tag) => selectedTags.includes(tag));
}
