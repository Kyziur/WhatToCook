import { Injectable } from '@angular/core';
import { RecipeService } from '../recipe.service';
import { BehaviorSubject, filter, map, Observable } from 'rxjs';
import { RecipeCard } from '../recipe-card/recipe-card.component';

@Injectable()
export class RecipeListService {
  private recipeCards = new BehaviorSubject<RecipeCard[]>([]);
  public recipeCards$: Observable<RecipeCard[]> =
    this.recipeCards.asObservable();

  constructor(private service: RecipeService) {
    this.service
      .get()
      .pipe(
        map(recipes => {
          return recipes.map(recipe => ({
            ...recipe,
            isSelected: false,
          }));
        })
      )
      .subscribe(recipeCards => {
        this.recipeCards.next(recipeCards);
      });
  }

  toggleSelect(id: number) {
    const recipe = this.recipeCards.value.find(x => x.id === id);
    if (!recipe) {
      return;
    }

    recipe.isSelected = !recipe.isSelected;
    this.recipeCards.next(this.recipeCards.value);
  }

  getSelected$() {
    return this.recipeCards$.pipe(
      map(recipes => recipes.filter(recipe => recipe.isSelected))
    );
  }

  select(ids: number[]) {
    this.recipeCards.value.forEach(recipe => {
      recipe.isSelected = ids.includes(recipe.id);
    });
  }
}
