import {Component} from '@angular/core';
import {FormControl, FormGroup, FormBuilder, FormArray, AbstractControl, ValidationErrors, Validators} from '@angular/forms';
import {PlanOfMeals} from './plan-of-meals';
import {ActivatedRoute, Router} from '@angular/router';
import {HttpClient} from '@angular/common/http';
import {MealPlanningService} from "../meal-planning.service";

function notWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
  const isWhitespace = (control.value || '').trim().length === 0;
  return isWhitespace ? { 'whitespace': true } : null;
}
// Check if 'toDate' is greater than 'fromDate'
function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const fromDate = new Date(group.get('fromDate')?.value);
  const toDate = new Date(group.get('toDate')?.value);
  return toDate >= fromDate ? null : { dateMismatch: true };
}

// Check if the date is not in the past
function notPastDateValidator(control: AbstractControl): ValidationErrors | null {
  
  const selectedDate = new Date(control.value);
  const currentDate = new Date();
  currentDate.setHours(0, 0, 0, 0); // Reset time to start of day
  return selectedDate >= currentDate ? null : { pastDate: true };
}
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

  get recipesControls(): FormArray<FormControl<string>> {
    return this.mealPlanForm?.get('recipes') as FormArray<FormControl<string>>;
  }


  viewMealPlanDetails(name: string | undefined) {
    if (name === undefined) {
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
    }
    ;
    this.mealPlanForm = this.fb.group({
      name: this.createStringControl(planOfMeals.name),
      fromDate: this.fb.nonNullable.control(planOfMeals.fromDate),
      toDate: this.fb.nonNullable.control(planOfMeals.toDate),
      recipes: this.fb.nonNullable.array([] as FormControl<string>[])
    })
  }

  createStringControl(value: string | undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }
  protected readonly MealPlanningService = MealPlanningService;
}

