import { Component, Input } from '@angular/core';
import { RecipeListService } from './recipe-list.service';
import {
  RecipeCard,
  RecipeCardComponent,
} from '../recipe-card/recipe-card.component';
import { NgFor, AsyncPipe } from '@angular/common';
import { SearchComponent } from '../../shared/search/search.component';
import { Observable, map, of } from 'rxjs';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  standalone: true,
  imports: [SearchComponent, NgFor, RecipeCardComponent, AsyncPipe],
})
export class RecipeListComponent {
  @Input() allowSelection = false;

  recipes: Observable<RecipeCard[]> = of([]);

  constructor(public service: RecipeListService) {
    this.service.recipeCards$.pipe(
      map(x => x.sort((a, b) => Number(b) - Number(a)))
    );
  }
}
