import { Component, OnInit } from '@angular/core';
import { RecipeService } from '../recipe.service';
import { Recipe } from '../Recipe';
import { CreateRecipe } from '../recipe-view/CreateRecipe';
@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss']
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[]= [];

constructor(private recipeService: RecipeService){}
  ngOnInit(): void{
      //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
      this.recipeService.get().subscribe(
        (recipes) => {
          console.log("Received recipes:", recipes);
          this.recipes = recipes;
        },
        (error) => {
          console.error(error);
        }
      );
}}
