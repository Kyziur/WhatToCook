import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { CreateRecipe } from '../recipe-view/CreateRecipe';

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  styleUrls: ['./recipe-card.component.scss']
})

export class RecipeCardComponent {
  @Input()
  recipe?: Recipe;


  //redirect to details of each recipe
  constructor (private router: Router){}

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
}
 // recipeCard: CreateRecipe | undefined;

