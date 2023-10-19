import { Component, OnInit } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import { DayWiseIngredients, shoppingListResponse } from './shopping-list-response.component';
import { PlanOfMeals } from '../meal-plan-creator/plan-of-meals';

@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  styleUrls: ['./shopping-list.component.scss']
})
export class ShoppingListComponent implements OnInit {
  mealPlans: PlanOfMeals[] = [];
  selectedMealPlanId: number | null = null;
  shoppingListByDate?: shoppingListResponse;
  isListView : boolean = true;
  dayWiseShoppingList: DayWiseIngredients[] = [];
  constructor(
      private mealPlanService: MealPlanningService,
  ) {}

  ngOnInit(): void {
    this.mealPlanService.getMealPlans().subscribe(plans => {
        this.mealPlans = plans;
    });
}
toggleView() {
  this.isListView = !this.isListView;
}
onMealPlanSelected() {
  if (this.selectedMealPlanId) {
    this.mealPlanService.getIngredientsForShoppingList(this.selectedMealPlanId)
    .subscribe(list => {
        this.shoppingListByDate = list;
        this.dayWiseShoppingList = list.dayWiseIngredientsList || [];
        console.log(this.shoppingListByDate); 
    });
  }
}
}