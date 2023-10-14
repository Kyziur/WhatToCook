import { Component } from '@angular/core';
import { RecipeListService } from './recipe-list.service';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss'],
})
export class RecipeListComponent {
  constructor(public service: RecipeListService) {}
}
