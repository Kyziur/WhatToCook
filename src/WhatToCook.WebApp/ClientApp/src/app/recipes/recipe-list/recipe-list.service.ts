import { Injectable } from '@angular/core';
import { RecipeCard } from '../recipe-card/recipe-card.component';
import { RecipeService } from '../recipe.service';

@Injectable({
  providedIn: 'root',
})
export class RecipeListService {
  public showSelectButton = false;
  public recipeCards: RecipeCard[] = [];

  public getSelectedRecipes() {
    return this.recipeCards.filter(recipe => recipe.isSelected);
  }

  constructor(private recipeService: RecipeService) {
    this.recipeService.get().subscribe({
      next: recipes => {
        console.log('Received recipes:', recipes);
        this.recipeCards = recipes.map(recipe => ({
          ...recipe,
          isSelected: false,
        }));
      },
      error: error => {
        console.error(error);
      },
    });
  }
}
