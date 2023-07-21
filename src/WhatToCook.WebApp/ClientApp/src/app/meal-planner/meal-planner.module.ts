import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {RouterModule, Routes} from "@angular/router";
import {PlanSelectComponent} from "./plan-select/plan-select.component";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {MealPlanCreatorComponent} from "./meal-plan-creator/meal-plan-creator.component";
import { MealPlanningComponent } from './meal-planning/meal-planning.component';
import {RecipesModule} from "../recipes/recipes.module";
import {SharedModule} from "../shared/shared.module";

const routes: Routes = [
  {
    path: 'meal-plan/new',
    component: MealPlanningComponent,
  },
  {
    path: 'meal-plan/:name',
    component: MealPlanCreatorComponent,
  },
]

@NgModule({
  declarations: [
    MealPlanCreatorComponent,
    PlanSelectComponent,
    MealPlanningComponent
  ],
  exports: [
    PlanSelectComponent
  ],
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        ReactiveFormsModule,
        RecipesModule,
        FormsModule,
        SharedModule,
    ]
})
export class MealPlannerModule { }
