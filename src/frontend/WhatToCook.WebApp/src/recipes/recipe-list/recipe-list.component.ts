import { Component, Input } from '@angular/core';
import { RecipeListService } from './recipe-list.service';
import {
  RecipeCard,
  RecipeCardComponent,
} from '../recipe-card/recipe-card.component';
import { NgFor, AsyncPipe } from '@angular/common';
import { SearchComponent } from '../../shared/search/search.component';
import { map } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  standalone: true,
  imports: [SearchComponent, NgFor, RecipeCardComponent, AsyncPipe],
  providers: [RecipeListService],
})
export class RecipeListComponent {
  @Input() allowSelection = false;

  recipes: RecipeCard[] = [];
  tags: string[] = [];
  ingredients: string[] = [];
  
  constructor(public service: RecipeListService) {
    this.service.recipeCards$
      .pipe(
        takeUntilDestroyed(),
        map(x => x.sort((a, b) => Number(b) - Number(a)))
      )
      .subscribe(recipes => {
        this.recipes = recipes;
        const tags = new Set<string>();
        const ingredients = new Set<string>();

        this.recipes.forEach(recipe => {
          recipe.tags.forEach(tag => tags.add(tag));
          recipe.ingredients.forEach(ing => ingredients.add(ing));
        });

        this.tags = Array.from(tags);
        this.ingredients = Array.from(ingredients);
      });
  }

  toggleSelection($event: Pick<RecipeCard, 'id'>) {
    this.service.toggleSelect($event.id);
  }
}
