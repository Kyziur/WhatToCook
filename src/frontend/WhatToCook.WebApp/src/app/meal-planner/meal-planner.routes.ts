import { Routes } from '@angular/router';
import { MealPlanCreatorComponent } from './meal-plan-creator/meal-plan-creator.component';
import { MealPlanListComponent } from './meal-plan-list/meal-plan-list.component';
import { ShoppingListComponent } from './shopping-list/shopping-list.component';

export const MEAL_PLANNER_ROUTES: Routes = [
  {
    path: 'meal-plans',
    component: MealPlanListComponent,
  },
  {
    path: 'meal-plans/new',
    component: MealPlanCreatorComponent,
  },
  {
    path: 'meal-plans/:name',
    component: MealPlanCreatorComponent,
  },
  {
    path: 'shopping-list',
    component: ShoppingListComponent,
  },
];
