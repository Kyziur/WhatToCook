import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Recipe } from './Recipe';
import { CreateRecipe } from './recipe-view/CreateRecipe';

@Injectable({
  providedIn: 'root',
})
export class RecipeService {
  recipeUrl = '';
  constructor(
    private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.recipeUrl = this.baseUrl + 'api/v1/Recipe';
  }

  create(recipe: CreateRecipe) {
    return this.httpClient.post<CreateRecipe>(
      this.baseUrl + 'api/v1/Recipe',
      recipe
    );
  }

  get(): Observable<Recipe[]> {
    return this.httpClient.get<Recipe[]>(`${this.baseUrl}api/v1/Recipe`);
  }

  getByName(name: string): Observable<Recipe> {
    return this.httpClient.get<Recipe>(`${this.recipeUrl}/${name}`);
  }

  update(recipe: CreateRecipe) {
    return this.httpClient.put<CreateRecipe>(
      this.baseUrl + 'api/v1/Recipe',
      recipe
    );
  }

  deleteRecipe(id: number) {
    return this.httpClient.delete(this.baseUrl + 'api/v1/Recipe/' + id);
  }
}
