import {Component} from '@angular/core';
import {FormControl, FormGroup, FormBuilder, FormArray, AbstractControl, ValidationErrors, Validators} from '@angular/forms';
import {PlanOfMeals} from './plan-of-meals';
import {ActivatedRoute, Router} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {MealPlanningService} from "../meal-planning.service";
import {dateRangeValidator, notPastDateValidator } from './date-validators.component';
import {notWhitespaceValidator } from 'src/app/not-white-space-validator.component';

@Component({
  selector: 'app-meal-creator',
  templateUrl: './meal-plan-creator.component.html',
  styleUrls: ['./meal-plan-creator.component.scss']
})

export class MealPlanCreatorComponent {
  hasInteractedWithFromDate: boolean = false;
  hasInteractedWithToDate: boolean = false;
  mealPlanForm: FormGroup = new FormGroup({
    name: this.fb.nonNullable.control("", [Validators.required, notWhitespaceValidator]),
    fromDate: this.fb.nonNullable.control(new Date(),[Validators.required, notPastDateValidator]),
    toDate: this.fb.nonNullable.control(new Date(), [Validators.required, notPastDateValidator]),
    recipes: this.fb.nonNullable.array([] as FormControl<string>[])
  }, { validators: dateRangeValidator });


  handleSuccesfulSave() {
    this.router.navigate(['recipes'])
  }

  constructor(private fb: FormBuilder, private router: Router, public mealPlanService: MealPlanningService, private http: HttpClient, private route: ActivatedRoute) {
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
  createStringControl(value: string | undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }
  protected readonly MealPlanningService = MealPlanningService;
}

