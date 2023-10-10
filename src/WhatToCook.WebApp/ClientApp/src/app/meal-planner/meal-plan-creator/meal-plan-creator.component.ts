import {Component} from '@angular/core';
import {FormArray, FormBuilder, FormControl, FormGroup, Validators} from '@angular/forms';
import {CreatePlanOfMeals} from './plan-of-meals';
import {Router} from '@angular/router';
import {MealPlanningService} from "../meal-planning.service";
import {dateRangeValidator, notPastDateValidator} from './date-validators.component';
import {notWhitespaceValidator} from 'src/app/not-white-space-validator.component';

export interface MealPlanForm {
  name: FormControl<string>
  fromDate: FormControl<string | null>;
  toDate: FormControl<string | null>;
  recipes: FormArray<FormControl<string>>;
}

@Component({
  selector: 'app-meal-creator',
  templateUrl: './meal-plan-creator.component.html',
  styleUrls: ['./meal-plan-creator.component.scss']
})

export class MealPlanCreatorComponent {
  selectedDate: Date | undefined;
  mealPlanForm: FormGroup<MealPlanForm> = new FormGroup({
    name: this.fb.nonNullable.control("", [Validators.required, notWhitespaceValidator]),
    fromDate: this.fb.control<string | null>(null, [Validators.required, notPastDateValidator]),
    toDate: this.fb.control<string | null>(null, [Validators.required, notPastDateValidator]),
    recipes: this.fb.nonNullable.array([] as FormControl<string>[])
  }, {validators: dateRangeValidator});


  get fromDate() {
    return this.mealPlanForm.controls.fromDate;
  }

  get toDate() {
    return this.mealPlanForm.controls.toDate;
  }

  get name() {
    return this.mealPlanForm.controls.name;
  }

  handleSuccessfulSave() {
    this.router.navigate(['recipes'])
  }

  constructor(private fb: FormBuilder, private router: Router, public mealPlanService: MealPlanningService) {
    this.fromDate.valueChanges.subscribe(_ => this.changedDateRangeHandler());
    this.toDate.valueChanges.subscribe(_ => this.changedDateRangeHandler());
  }

  submit() {
    if (!this.mealPlanForm || this.mealPlanForm.invalid) {
      return;
    }

    if (!this.fromDate.value || !this.toDate.value) {
      return;
    }

    const request: CreatePlanOfMeals = {
      name: this.name.value,
      fromDate: new Date(this.fromDate.value),
      toDate: new Date(this.toDate.value),
    };

    console.error('request', request);


    // this.mealPlanService.createMealPlan(request).subscribe(_ => this.handleSuccessfulSave());
  }

  getDaysFromSelectedDates() {
    if (!this.toDate.value || !this.fromDate.value) {
      return [];
    }

    return this.generateRangeOfDates(new Date(this.fromDate.value), new Date(this.toDate.value));
  }

  selectDateClickHandler($event: MouseEvent, date: Date){
    $event.preventDefault();
    this.selectedDate = date;
  }

  changedDateRangeHandler(){
    this.selectedDate = undefined;
  }

  generateRangeOfDates(from: Date, to: Date) {
    let currentDate = from;
    let rangeDates: Date[] = [];
    while (currentDate <= to) {
      rangeDates.push(new Date(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }
    return rangeDates;
  }
}

