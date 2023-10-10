import { Component, Input, OnInit } from '@angular/core';
import { RecipeService } from '../recipe.service';
import { Recipe } from '../Recipe';
import { SelectButton } from '../recipe-card/recipe-card.component';

export type filterRecipePredicate = (recipe: Recipe) => boolean;

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss'],
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[] = [];
  @Input() recipeCardSelectButton: SelectButton;
  @Input() filterMethod?: filterRecipePredicate;

  constructor(private recipeService: RecipeService) {}

  ngOnInit(): void {
    this.recipeService.get().subscribe({
      next: recipes => {
        console.log('Received recipes:', recipes);
        this.recipes = this.filterMethod
          ? recipes.filter(recipe => this.filterMethod?.(recipe))
          : recipes;
      },
      error: error => {
        console.error(error);
      },
    });
  }
}
