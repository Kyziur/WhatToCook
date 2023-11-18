import { NgModule } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MealPlanCreatorComponent } from './meal-plan-creator/meal-plan-creator.component';
import { RecipesModule } from '../recipes/recipes.module';
import { SharedModule } from '../shared/shared.module';
import { ShoppingListComponent } from './shopping-list/shopping-list.component';
import { MealPlanListComponent } from './meal-plan-list/meal-plan-list.component';
import { RecipeListService } from '../recipes/recipe-list/recipe-list.service';
import { InputDateComponent } from '../shared/input-date/input-date.component';

const routes: Routes = [
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

@NgModule({
  declarations: [
    MealPlanCreatorComponent,
    MealPlanListComponent,
    ShoppingListComponent,
  ],
  exports: [],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    RecipesModule,
    FormsModule,
    SharedModule,
    NgOptimizedImage,
    InputDateComponent,
  ],
  providers: [RecipeListService],
})
export class MealPlannerModule {}
