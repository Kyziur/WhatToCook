import { Component, Input, isDevMode } from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { mapTimeToPrepareToBadge } from '../TimeToPrepare';
import { RecipeListService } from '../recipe-list/recipe-list.service';
import { BadgeComponent } from '../../shared/badge/badge.component';
import { NgIf, NgFor, NgOptimizedImage } from '@angular/common';

export interface RecipeCard extends Recipe {
  isSelected: boolean;
}

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  standalone: true,
  imports: [NgIf, BadgeComponent, NgFor, NgOptimizedImage],
})
export class RecipeCardComponent {
  @Input() recipe?: RecipeCard;
  @Input() showSelectButton = false;

  constructor(
    private router: Router,
    private recipeListService: RecipeListService
  ) {}

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
      this.recipeListService.toggleSelect(this.recipe.id);
    }
  }
}
