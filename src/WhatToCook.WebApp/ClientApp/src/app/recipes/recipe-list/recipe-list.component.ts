import { Component, Input } from '@angular/core';
import { RecipeListService } from './recipe-list.service';
import { RecipeCardComponent } from '../recipe-card/recipe-card.component';
import { NgFor, AsyncPipe } from '@angular/common';
import { SearchComponent } from '../../shared/search/search.component';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  standalone: true,
  imports: [SearchComponent, NgFor, RecipeCardComponent, AsyncPipe],
})
export class RecipeListComponent {
  @Input() allowSelection = false;

  constructor(public service: RecipeListService) {}
}
