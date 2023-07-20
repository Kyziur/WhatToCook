import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import {MealPlanningService} from "../../meal-planner/meal-planning.service";


@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss']
})

export class RecipeCardComponent {
  @Input()
  recipe?: Recipe;
  constructor (private router: Router, private mealPlanningService: MealPlanningService){}

  viewRecipeDetails(name:string | undefined){
    if(name === undefined){
      return
    }
    this.router.navigate([`/recipes/${name}`])
  }

  getImagePath() {
    if(!this.recipe){
      return '';
    }

    return this.recipe.imagePath
  }

  onSelect(){
    if(this.recipe === undefined){
      return
    }

    if(this.mealPlanningService.selectedRecipes.includes(this.recipe)){
      this.mealPlanningService.selectedRecipes = this.mealPlanningService.selectedRecipes.filter(x => x.name !== this.recipe?.name)
    } else{
      this.mealPlanningService.selectRecipe(this.recipe)
    }
  }
}

