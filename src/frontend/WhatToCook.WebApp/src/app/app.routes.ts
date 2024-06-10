import { Routes } from '@angular/router';
import { MEAL_PLANNER_ROUTES } from '../meal-planner/meal-planner.routes';
import { RECIPES_ROUTES } from '../recipes/recipes.routes';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'recipes',
    pathMatch: 'full',
  },
  ...RECIPES_ROUTES,
  ...MEAL_PLANNER_ROUTES,
];
