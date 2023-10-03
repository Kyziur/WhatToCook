import {Component, Input} from '@angular/core';
import {Router} from '@angular/router';
import {Recipe} from '../Recipe';
import {MealPlanningService} from "../../meal-planner/meal-planning.service";
import {Badge} from "../../shared/badge/badge.component";
import {mapTimeToPrepareToBadge} from "../TimeToPrepare";


@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss']
})

export class RecipeCardComponent {
  @Input()
  recipe?: Recipe;
  selected?: boolean;

  constructor(private router: Router, private mealPlanningService: MealPlanningService) {
  }

  viewRecipeDetails(name: string | undefined) {
    if (name === undefined) {
      return
    }
    this.router.navigate([`/recipes/${name}`])
  }

  getImagePath() {
    if (!this.recipe) {
      return '';
    }

    return this.recipe.imagePath
  }

  getTimeToPrepareBadge(recipe: Recipe) {
    return mapTimeToPrepareToBadge(recipe.timeToPrepare);
  }

  get tags() {
    return this.recipe?.tags ?? ['test1', 'test2'];
  }

  onSelect() {
    if (this.recipe === undefined) {
      return
    }
    if (this.selected) {
      this.mealPlanningService.selectRecipe(this.recipe)
    } else {
      this.mealPlanningService.selectedRecipes = this.mealPlanningService.selectedRecipes.filter(x => x.id !== this.recipe?.id)
    }
  }
}
