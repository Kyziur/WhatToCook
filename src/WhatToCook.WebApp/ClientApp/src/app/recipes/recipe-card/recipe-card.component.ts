import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { MealPlanningService } from '../../meal-planner/meal-planning.service';
import { mapTimeToPrepareToBadge } from '../TimeToPrepare';

export interface RecipeCard extends Recipe {
  isSelected: boolean;
}

export interface SelectButton {
  show: boolean;
  onClick?: (recipe: Recipe) => void;
}

export const DEFAULT_SELECT_BUTTON: SelectButton = {
  show: false,
  onClick: undefined,
};

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss'],
})
export class RecipeCardComponent {
  @Input() recipe?: RecipeCard;
  @Input() showSelectButton = false;

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

  get tags() {
    return this.recipe?.tags ?? ['test1', 'test2'];
  }

  selectClickHandler() {
    if (this.recipe) {
      this.recipe.isSelected = !this.recipe.isSelected;
    }
  }

  getSelectIcon(recipe: RecipeCard) {
    return recipe.isSelected
      ? 'assets/icons/plus-square-selected.svg'
      : 'assets/icons/plus-square.svg';
  }
}
