import { Component, Input } from '@angular/core';
import { MealPlanningService } from 'src/app/layout/sidebar/meal-planning/meal-planning.service';
import { PlanOfMeals } from 'src/app/layout/sidebar/meal-planning/plan-of-meals';
@Component({
  selector: 'app-plan-select',
  templateUrl: './plan-select.component.html',
  styleUrls: ['./plan-select.component.scss']
})
export class PlanSelectComponent {
  @Input()
  mealPlan: PlanOfMeals[]=[];
  constructor(private mealPlanService: MealPlanningService){}

  ngOnInit(): void{
    //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
    this.mealPlanService.getMealPlan().subscribe(
      (mealPlan) => {
        console.log("Received plans:", mealPlan);
        this.mealPlan = mealPlan;
      },
      (error) => {
        console.error(error);
      }
    );
}

}