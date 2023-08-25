import {Component, EventEmitter, Input, Output} from '@angular/core';
import {MealPlanningService} from "../meal-planning.service";
import {PlanOfMeals, UpdatePlanOfMeals} from "../meal-plan-creator/plan-of-meals";

const EMPTY_MEAL_PLAN: PlanOfMeals = {
  name: "Zaznacz",
  id: -1,
  recipes: [],
  toDate: new Date(),
  fromDate: new Date()
};

@Component({
  selector: 'app-plan-select',
  templateUrl: './plan-select.component.html',
  styleUrls: ['./plan-select.component.scss']
})
export class PlanSelectComponent {
  mealPlans: PlanOfMeals[] = [];
  selectedMealPlan?: PlanOfMeals;

  @Output() onSelectionChange: EventEmitter<PlanOfMeals> = new EventEmitter<PlanOfMeals>();

  constructor(private mealPlanService: MealPlanningService) {
  }

  ngOnInit(): void {
    this.mealPlanService.getMealPlan().subscribe(
      (mealPlans) => {
        console.log("Received plans:", mealPlans);
        this.mealPlans = [EMPTY_MEAL_PLAN, ...mealPlans];
        this.selectedMealPlan = this.mealPlans[0];

      },
      (error) => {
        console.error(error);
      }
    );
  }

  selectedMealPlanChanged() {
    this.onSelectionChange.emit(this.selectedMealPlan);
  }
    onSubmit() {
   this.mealPlanService.selectedRecipes
   this.selectedMealPlan?.name
   if(this.selectedMealPlan === undefined || this.selectedMealPlan === null){
    return 
   }
   const mealPlan: UpdatePlanOfMeals = {
    id: this.selectedMealPlan.id,
    fromDate: this.selectedMealPlan.fromDate,
    toDate: this.selectedMealPlan.toDate,
    name: this.selectedMealPlan.name,
    recipes: this.mealPlanService.selectedRecipes.map(recipe => {
      return recipe.name
    })
   }
   this.mealPlanService.update(mealPlan).subscribe();
  }
}
