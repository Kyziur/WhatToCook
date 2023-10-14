import { Component, OnInit } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import { ShoppingListResponse } from './shopping-list-response.component';
import { PlanOfMeals } from '../meal-plan-creator/plan-of-meals';

@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  styleUrls: ['./shopping-list.component.scss']
})
export class ShoppingListComponent implements OnInit {
  mealPlans: PlanOfMeals[] = [];
  selectedMealPlanId: number | null = null;
  shoppingListByDate: ShoppingListResponse[] = [];

  constructor(
      private mealPlanService: MealPlanningService,
  ) {}

  ngOnInit(): void {
    this.mealPlanService.getMealPlans().subscribe(plans => {
        this.mealPlans = plans;
    });
}

onMealPlanSelected() {
  if (this.selectedMealPlanId) {
      this.mealPlanService.getIngredientsForShoppingList(this.selectedMealPlanId)
          .subscribe(response => {
              console.log(response);  
              this.shoppingListByDate = [response];
          });
  }
}
}