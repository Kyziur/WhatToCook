import { Component } from '@angular/core';
import { AppModule } from 'src/app/app.module';
import {MealPlanningService} from "../../../meal-planner/meal-planning.service";

@Component({
  selector: 'app-menu-list',
  templateUrl: './menu-list.component.html',
  styleUrls: ['./menu-list.component.scss']

})
export class MenuListComponent {

  constructor(private mealPlanningService: MealPlanningService) {
  }
  showMealPlanningModal() {
    this.mealPlanningService.showMealPlanningModal();
  }
}
