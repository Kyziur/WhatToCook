import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Recipe } from './Recipe';
import { CreateRecipe } from './recipe-view/CreateRecipe';
import { PlanOfMeals } from '../layout/sidebar/meal-planning/plan-of-meals';
import { HttpEvent, HttpEventType } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  recipeUrl = "";
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.recipeUrl = this.baseUrl + 'api/v1/Recipe';
  }
  create(recipe: CreateRecipe) {
    return this.httpClient.post<CreateRecipe>(this.baseUrl + 'api/v1/Recipe', recipe)
  }
  get(): Observable<Recipe[]> {
    return this.httpClient.get<Recipe[]>(`${this.baseUrl}api/v1/Recipe`);
  }

  getByName(name: string): Observable<Recipe> {
    return this.httpClient.get<Recipe>(`${this.recipeUrl}/${name}`);
  }
  update(recipe: CreateRecipe) {
    return this.httpClient.put<CreateRecipe>(this.baseUrl + 'api/v1/Recipe', recipe)
  }

  createMealPlan(planOfMeals : PlanOfMeals){
    
  }

  getMealPlan(planOfMeals: PlanOfMeals) {
    return this.httpClient.get<PlanOfMeals>('$(this.recipeUrl)/${name}');
  }
}




