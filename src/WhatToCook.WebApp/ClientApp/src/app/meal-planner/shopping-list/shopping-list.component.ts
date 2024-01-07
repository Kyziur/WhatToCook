import { Component, Input, OnInit } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import { ShoppingListResponse } from './shopping-list-response.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgIf, NgFor, DatePipe } from '@angular/common';

@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  standalone: true,
  imports: [NgIf, ReactiveFormsModule, FormsModule, NgFor, DatePipe],
})
export class ShoppingListComponent {
  @Input() set mealPlanId(value: number | undefined) {
    this._mealPlanId = value;
    this.fetchIngredients();
  }

  private _mealPlanId?: number;
  shouldDisplayEntireList = false;

  entireShoppingList: string[] = [];
  shoppingList: ShoppingListResponse = {
    fromDate: new Date(),
    toDate: new Date(),
    ingredientsPerDay: [],
  };

  constructor(private mealPlanService: MealPlanningService) {}

  fetchIngredients(): void {
    if (!this._mealPlanId) {
      return;
    }
    this.mealPlanService
      .getIngredientsForShoppingList(this._mealPlanId)
      .subscribe(response => {
        this.shoppingList = response;
        this.entireShoppingList = response.ingredientsPerDay.flatMap(
          x => x.ingredients
        );
      });
  }
}
