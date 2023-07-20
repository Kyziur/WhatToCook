import {Component, Input, OnInit} from '@angular/core';
import {RecipeService} from '../recipe.service';
import {Recipe} from '../Recipe';
import {CreateRecipe} from '../recipe-view/CreateRecipe';

export type filterRecipePredicate = (recipe: Recipe) => boolean;

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss']
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];

  @Input() filterMethod?: filterRecipePredicate;

  constructor(private recipeService: RecipeService) {
  }

  ngOnInit(): void {
    //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
    this.recipeService.get().subscribe(
      (recipes) => {
        console.log("Received recipes:", recipes);
        this.recipes = this.filterMethod ? recipes.filter(recipe => this.filterMethod?.(recipe)) : recipes;
      },
      (error) => {
        console.error(error);
      }
    );
  }
}
