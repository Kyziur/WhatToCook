import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MealPlanningService } from '../meal-planning.service';
import { map } from 'rxjs';
import { shorten } from '../../common/functions/shorten';
import { NgFor, NgOptimizedImage, AsyncPipe, DatePipe } from '@angular/common';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroArrowRight, heroListBullet } from '@ng-icons/heroicons/outline';
import { ModalComponent } from '../../shared/modal/modal.component';
import { ShoppingListComponent } from '../shopping-list/shopping-list.component';

export interface MealPlanItem {
  id: number;
  name: string;
  fromDate: Date;
  toDate: Date;
  numberOfRecipes: number;
}

interface ShoppingListView {
  isVisible: boolean;
  mealPlanId: number;
}
@Component({
  selector: 'app-meal-plan-list',
  templateUrl: './meal-plan-list.component.html',
  styleUrls: ['./meal-plan-list.component.scss'],
  standalone: true,
  imports: [
    NgFor,
    NgOptimizedImage,
    AsyncPipe,
    DatePipe,
    NgIconComponent,
    ModalComponent,
    ShoppingListComponent,
  ],
  providers: [provideIcons({ heroListBullet, heroArrowRight }), DatePipe],
})
export class MealPlanListComponent {
  constructor(
    private router: Router,
    private service: MealPlanningService
  ) {}

  getPlans$ = this.service.getAll().pipe(
    map(response =>
      response.mealPlans.map(
        plan =>
          ({
            ...plan,
            numberOfRecipes: plan.recipes.length,
          }) as MealPlanItem
      )
    )
  );

  shoppingList: ShoppingListView = {
    isVisible: false,
    mealPlanId: 0,
  };

  public shortenName(name: string) {
    return shorten(name, 30);
  }

  showShoppingList(mealPlan: MealPlanItem) {
    this.shoppingList.isVisible = true;
    this.shoppingList.mealPlanId = mealPlan.id;
  }

  navigateToMealPlan(mealPlan: MealPlanItem) {
    this.router.navigate([`/meal-plans/${mealPlan.name}`]);
  }
}
