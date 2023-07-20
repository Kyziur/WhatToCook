import { Component } from '@angular/core';
import {PlanOfMeals} from "../meal-plan-creator/plan-of-meals";

@Component({
  selector: 'app-meal-planning',
  templateUrl: './meal-planning.component.html',
  styleUrls: ['./meal-planning.component.scss']
})
export class MealPlanningComponent {

  onMealPlanSelectionChange($event: PlanOfMeals) {
    console.error("HALOOOO", $event);
  }
}
