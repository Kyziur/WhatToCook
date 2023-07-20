import { Component } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, FormArray, Form } from '@angular/forms';
import { RecipeViewComponent } from '../../../recipes/recipe-view/recipe-view.component'
import { Recipe } from 'src/app/recipes/Recipe';
import { PlanOfMeals } from './plan-of-meals';
import { ActivatedRoute, Router } from '@angular/router';
import { RecipeService } from 'src/app/recipes/recipe.service';
import { HttpClient } from '@angular/common/http';
import { MealPlanningService } from './meal-planning.service';

@Component({
  selector: 'app-meal-planning',
  templateUrl: './meal-planning.component.html',
  styleUrls: ['./meal-planning.component.scss']
})

export class MealPlanningComponent {
  mealPlanForm: FormGroup = new FormGroup ({
    name: this.fb.nonNullable.control(""), 
    fromDate: this.fb.nonNullable.control(new Date()),
    toDate: this.fb.nonNullable.control(new Date()),
    recipes: this.fb.nonNullable.array([]as FormControl<string>[])});


  handleSuccesfulSave() {
    this.router.navigate(['recipes'])
  }
  constructor(private fb: FormBuilder, private router: Router, private mealPlanService: MealPlanningService, private http: HttpClient, private route: ActivatedRoute) {
  }

  get recipesControls(): FormArray<FormControl<string>> {
    return this.mealPlanForm?.get('recipes') as FormArray<FormControl<string>>;
  }



  viewMealPlanDetails(name:string | undefined){
    if(name === undefined){
      return
    }
    this.router.navigate([`/meal-plans/${name}`])
  }

  submit() {
    const meaPlanLoad = new FormData();
    let form = this.mealPlanForm?.getRawValue();
    if (!form) {
      return;
    }
    for (const [key, value] of Object.entries(form)) {
      if (Array.isArray(value)) {
        for (var v = 0; v < value.length; v++) {
          meaPlanLoad.append(`${key}[${v}]`, value[v]);
        }
        this.mealPlanService.createMealPlan(this.mealPlanForm?.value as PlanOfMeals).subscribe((x) => this.handleSuccesfulSave());
      }
    }
  }
  loadFormData(planOfMeals?: PlanOfMeals) {
    if (!planOfMeals) {
      return
    };
    this.mealPlanForm = this.fb.group({
      name: this.createStringControl(planOfMeals.name),
      fromDate: this.fb.nonNullable.control(planOfMeals.fromDate),
      toDate: this.fb.nonNullable.control(planOfMeals.toDate),
      recipes: this.fb.nonNullable.array([]as FormControl<string>[])
    })
  }

  createStringControl(value: string | undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }
}

