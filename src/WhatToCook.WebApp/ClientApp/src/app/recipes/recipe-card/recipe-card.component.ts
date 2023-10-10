import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { MealPlanningService } from '../../meal-planner/meal-planning.service';
import { mapTimeToPrepareToBadge } from '../TimeToPrepare';

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
  @Input() recipe?: Recipe;
  @Input() selectButton?: SelectButton;

  constructor(
    private router: Router,
    private mealPlanningService: MealPlanningService
  ) {
    this.selectButton = DEFAULT_SELECT_BUTTON;
  }

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

  onSelect() {
    if (!this.recipe || !this.selectButton?.onClick) {
      return;
    }

    this.selectButton.onClick(this.recipe);
  }
}
