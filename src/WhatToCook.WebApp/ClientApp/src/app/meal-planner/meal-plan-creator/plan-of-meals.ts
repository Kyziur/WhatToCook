import { Recipe } from 'src/app/recipes/Recipe';

export interface PlanOfMeals {
  id: number;
  name: string;
  fromDate: Date;
  toDate: Date;
  plannedMealsForDay: PlanOfMealForDay[];
}

export interface PlanOfMealForDay {
  day: Date;
  recipesIds: number[];
}
export interface CreatePlanOfMeals {
  name: string;
  fromDate: Date;
  toDate: Date;
  recipes: PlanOfMealForDay[];
}
export interface UpdatePlanOfMeals {
  id: number;
  name: string;
  fromDate: Date;
  toDate: Date;
  recipes: string[];
}
