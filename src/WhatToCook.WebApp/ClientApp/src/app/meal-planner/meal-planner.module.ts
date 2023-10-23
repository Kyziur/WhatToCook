import { NgModule } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { PlanSelectComponent } from './plan-select/plan-select.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MealPlanCreatorComponent } from './meal-plan-creator/meal-plan-creator.component';
import { MealPlanningComponent } from './meal-planning/meal-planning.component';
import { RecipesModule } from '../recipes/recipes.module';
import { SharedModule } from '../shared/shared.module';
import { MealPlanListComponent } from './meal-plan-list/meal-plan-list.component';

const routes: Routes = [
  {
    path: 'meal-plans/new',
    component: MealPlanCreatorComponent,
  },
  {
    path: 'meal-plans/:id',
    component: MealPlanCreatorComponent,
  },
  {
    path: 'meal-plans',
    component: MealPlanListComponent,
  },
];

@NgModule({
  declarations: [
    MealPlanCreatorComponent,
    PlanSelectComponent,
    MealPlanningComponent,
    MealPlanListComponent,
  ],
  exports: [PlanSelectComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    RecipesModule,
    FormsModule,
    SharedModule,
    NgOptimizedImage,
  ],
})
export class MealPlannerModule {}
