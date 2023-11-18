import { Component, Input } from '@angular/core';
import { RecipeListService } from './recipe-list.service';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss'],
})
export class RecipeListComponent {
  @Input() allowSelection = false;

  constructor(public service: RecipeListService) {}
}
