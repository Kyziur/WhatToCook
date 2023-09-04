import { Component, Input, OnInit } from '@angular/core';
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
  selected?: boolean;
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

ngAfterContentInit(){
  if(this.recipe === undefined){
    return
  } console.error("recipe?", this.recipe)
 this.selected = this.mealPlanningService.selectedRecipes.some(x => x.id === this.recipe?.id); 
}

  onSelect(){
    if(this.recipe === undefined){
      return
    }
    if(this.selected){
      this.mealPlanningService.selectRecipe(this.recipe)
    }else{
      this.mealPlanningService.selectedRecipes = this.mealPlanningService.selectedRecipes.filter(x => x.id !== this.recipe?.id)
    }
   /* if(this.mealPlanningService.selectedRecipes.includes(this.recipe)){
      this.mealPlanningService.selectedRecipes = this.mealPlanningService.selectedRecipes.filter(x => x.id !== this.recipe?.id)
    } else{
      this.mealPlanningService.selectRecipe(this.recipe)
    }*/
    console.log("asf", this.mealPlanningService.selectedRecipes)
  }
  
}

