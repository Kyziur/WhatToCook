import { RecipeCard } from '../../recipes/recipe-card/recipe-card.component';

export interface MealPlanForDay {
  day: Date;
  recipes: RecipeCard[];
}
