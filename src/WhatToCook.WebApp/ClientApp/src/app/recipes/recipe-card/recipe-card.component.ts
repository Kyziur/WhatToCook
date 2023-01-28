import { Component, Input } from '@angular/core';

export interface RecipeCardViewModel {
  name: string;
  img: string;
  description: string;
 
}

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss']
})
export class RecipeCardComponent {
  @Input()
  recipeCard: RecipeCardViewModel | undefined;
}
