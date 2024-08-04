import { Component, Input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { SearchComponent } from '../../shared/components/search/search.component';
import { RecipeCardComponent } from '../recipe-card/recipe-card.component';
import { RecipeListService } from './recipe-list.service';
import { CommonModule } from '@angular/common';
import { RecipeCard } from '../recipe.types';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  standalone: true,
  imports: [CommonModule, SearchComponent, RecipeCardComponent],
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

  toggleSelection($event: number) {
    this.service.toggleSelect($event);
  }
}
