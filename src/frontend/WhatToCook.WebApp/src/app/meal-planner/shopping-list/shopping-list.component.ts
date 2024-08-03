import { CommonModule, DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MealPlanningService } from '../meal-planning.service';
import { ShoppingListResponse } from './shopping-list-response.component';

@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
})
export class ShoppingListComponent {
  @Input() set mealPlanId(value: number | undefined) {
    this._mealPlanId = value;
    this.fetchIngredients();
  }

  private _mealPlanId?: number;
  shouldDisplayEntireList = false;

  entireShoppingList: string[] = [];
  shoppingList: Map<string, string[]> = new Map<string, string[]>();

  constructor(
    private mealPlanService: MealPlanningService,
    private datePipe: DatePipe
  ) {}

  fetchIngredients(): void {
    if (!this._mealPlanId) {
      return;
    }
    this.mealPlanService
      .getIngredientsForShoppingList(this._mealPlanId)
      .subscribe(response => {
        this.shoppingList = this.groupShoppingItemsByDay(response);
        this.entireShoppingList = response.ingredientsPerDay.flatMap(
          x => x.ingredients
        );
      });
  }

  groupShoppingItemsByDay(list: ShoppingListResponse): Map<string, string[]> {
    const map = new Map<string, string[]>();
    const toFullDate = (date: Date) =>
      this.datePipe.transform(date, 'fullDate');

    console.error('list', list);
    for (const item of list.ingredientsPerDay) {
      const dayFormatted = toFullDate(item.day) ?? '';
      map.set(
        dayFormatted,
        (map.get(dayFormatted) || []).concat(item.ingredients)
      );
    }
    return map;
  }
}
