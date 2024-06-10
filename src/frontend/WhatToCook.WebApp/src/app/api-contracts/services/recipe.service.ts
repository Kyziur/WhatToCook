import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RecipeRequest } from '../model/recipeRequest';
import { RecipeResponse } from '../model/recipeResponse';

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

  create(recipe: RecipeRequest) {
    return this.httpClient.post<RecipeRequest>(
      this.baseUrl + 'api/v1/Recipe',
      recipe
    );
  }

  get(): Observable<RecipeResponse[]> {
    return this.httpClient.get<RecipeResponse[]>(
      `${this.baseUrl}api/v1/Recipe`
    );
  }

  getByName(name: string): Observable<RecipeResponse> {
    return this.httpClient.get<RecipeResponse>(`${this.recipeUrl}/${name}`);
  }

  update(recipe: RecipeRequest) {
    return this.httpClient.put<RecipeRequest>(
      this.baseUrl + 'api/v1/Recipe',
      recipe
    );
  }

  deleteRecipe(id: number) {
    return this.httpClient.delete(this.baseUrl + 'api/v1/Recipe/' + id);
  }
}
