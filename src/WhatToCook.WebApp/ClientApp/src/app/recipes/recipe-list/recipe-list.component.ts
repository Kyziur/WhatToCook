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

  constructor(public service: RecipeListService) {
    this.service.recipeCards$
      .pipe(
        takeUntilDestroyed(),
        map((x) => x.sort((a, b) => Number(b) - Number(a)))
      )
      .subscribe((recipes) => {
        this.recipes = recipes;
      });
  }
}
