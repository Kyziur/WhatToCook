import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit, Type } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

export interface CreateRecipe {
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: string;
}

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss']
})
export class RecipeViewComponent implements OnInit {
  timeToPrepareOptions = ["Short", "Medium", "Long"];
  constructor(private fb: FormBuilder, private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }
  //Add typing to this form
  //https://angular.io/guide/typed-forms
  //11. Each api endpoint should start with the following baseUrl + /api/v1/controller-name

  // recipeForm2 = new FormGroup({
  //   name: new FormControl('', { nonNullable: true }),
  //   ingredients: new FormArray([new FormControl('', { nonNullable: true })]),
  //   preperationDescription: new FormControl('', { nonNullable: true }),
  //   timeToPrepare: new FormControl('', { nonNullable: true }),
  //   acceptedPrivacyPolicy: new FormControl(false, { nonNullable: true })
  // })

  recipeForm = this.fb.group({
    name: this.fb.nonNullable.control(''),
    ingredients: this.fb.array([this.fb.nonNullable.control('')]),
    preperationDescription: this.fb.nonNullable.control(''),
    timeToPrepare: this.fb.nonNullable.control(''),
  })

  get ingredientsControls(): FormArray<FormControl<string>> {
    return this.recipeForm.get('ingredients') as FormArray<FormControl<string>>;
  }
  preview: string = '';
  preview2: string = '';

  ngOnInit(): void { }

  save() {
    this.preview = JSON.stringify(this.recipeForm.value);
    // debugger;
    const value = this.recipeForm.value;
    // this.httpClient.get(this.baseUrl + 'api/v1/WeatherForecast').subscribe(x => console.error('res', x));
    // this.httpClient.post<CreateRecipe>(this.baseUrl + 'api/v1/WeatherForecast', value).subscribe(response => {
    //   console.error('response from request', response)
    //   this.preview2 = JSON.stringify(response);
    // });
  }

  addIngredient() {
    this.ingredientsControls.push(this.fb.nonNullable.control(''));
  }
}