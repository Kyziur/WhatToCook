import { Inject, Injectable } from '@angular/core';
import { PlanOfMeals } from './plan-of-meals';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MealPlanningService {

  recipeUrl = "";
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.recipeUrl = this.baseUrl + 'api/v1/Recipe';
  }
  
  createMealPlan(planOfMeals : PlanOfMeals){
    
  }

  getMealPlan(planOfMeals: PlanOfMeals) {
    return this.httpClient.get<PlanOfMeals>('$(this.recipeUrl)/${name}');
  }
}
