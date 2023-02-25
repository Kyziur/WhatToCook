import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Recipe } from './Recipe';
import { CreateRecipe } from './recipe-view/CreateRecipe';


@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }
  create(recipe: CreateRecipe){
   return this.httpClient.post<CreateRecipe>(this.baseUrl + 'api/v1/Recipe', recipe)
  }
  get(): Observable<Recipe[]> {
    return this.httpClient.get<Recipe[]>(`${this.baseUrl}api/v1/Recipe`);
  }

}



