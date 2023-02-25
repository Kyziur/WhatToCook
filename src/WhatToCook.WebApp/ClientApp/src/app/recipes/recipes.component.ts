import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Recipe } from './Recipe';
import { RecipeService } from './recipe.service';
@Component({
  selector: 'app-recipes',
  templateUrl: './recipes.component.html',
  styleUrls: ['./recipes.component.scss']
})
export class RecipesComponent{

  recipe?: Recipe[];
    constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService
  ) {}
  
  ngOnInit(): void {
    this.recipeService.get().subscribe(
      (recipes) => {
        this.recipe = recipes;
      },
      (error) => {
        console.error(error);
      }
    );
  }
}
