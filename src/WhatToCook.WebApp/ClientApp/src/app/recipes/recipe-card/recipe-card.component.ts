import {
  Component,
  EventEmitter,
  Input,
  isDevMode,
  Output,
} from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { mapTimeToPrepareToBadge } from '../TimeToPrepare';

export interface RecipeCard extends Recipe {
  isSelected: boolean;
}

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss'],
})
export class RecipeCardComponent {
  @Input() recipe?: RecipeCard;
  @Input() selected = false;
  @Input() showSelectButton = false;
  @Output() recipeCardSelectionChange: EventEmitter<RecipeCard> =
    new EventEmitter();

  constructor(private router: Router) {}

  viewRecipeDetails(name: string | undefined) {
    if (name === undefined) {
      return;
    }
    this.router.navigate([`/recipes/${name}`]);
  }

  getImagePath() {
    if (!this.recipe) {
      return '';
    }

    return this.recipe.imagePath;
  }

  setDefaultImage() {
    if (this.recipe) {
      this.recipe.imagePath = 'Images/default_image.png';
    }
  }

  getTimeToPrepareBadge(recipe: Recipe) {
    return mapTimeToPrepareToBadge(recipe.timeToPrepare);
  }

  getTags(recipe: RecipeCard) {
    return isDevMode() ? recipe.tags ?? ['test1', 'test2'] : recipe.tags;
  }

  selectClickHandler() {
    if (this.recipe) {
      this.recipe.isSelected = !this.recipe.isSelected;
    }

    this.recipeCardSelectionChange.emit(this.recipe);
  }
}
