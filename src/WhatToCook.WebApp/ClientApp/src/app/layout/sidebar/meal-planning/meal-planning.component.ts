import { Component } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { RecipeViewComponent} from '../../../recipes/recipe-view/recipe-view.component'
import { Recipe } from 'src/app/recipes/Recipe';
import { PlanOfMeals } from './plan-of-meals';
import { ActivatedRoute, Router } from '@angular/router';
import { RecipeService } from 'src/app/recipes/recipe.service';
import { HttpClient } from '@angular/common/http';
@Component({
  selector: 'app-meal-planning',
  templateUrl: './meal-planning.component.html',
  styleUrls: ['./meal-planning.component.scss']
})
export class MealPlanningComponent {
  //recipes? : Recipe[];
  mealForm: FormGroup<{
    name: FormControl<string>;
    recipes: FormArray<FormControl<string>>;
    fromDate: FormControl<Date>;
    toDate: FormControl<Date>;
  }> | undefined

  handleSuccesfulSave() {
    this.router.navigate(['recipes'])
  }
  constructor(private fb: FormBuilder, private router: Router, private recipeService: RecipeService, private http: HttpClient) {
  }

  get recipesControls(): FormArray<FormControl<string>> {
    return this.mealForm?.get('recipes') as FormArray<FormControl<string>>;
  }

  submit() {
    const recipeload = new FormData();
    let form = this.mealForm?.getRawValue();
    this.recipeService.getMealPlan(this.mealForm?.value as PlanOfMeals).subscribe((x) => this.handleSuccesfulSave());
  }
}
