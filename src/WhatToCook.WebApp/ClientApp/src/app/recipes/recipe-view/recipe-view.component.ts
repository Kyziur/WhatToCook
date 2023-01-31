import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

export interface CreateRecipe {
  name: string;
  ingredients: string;
  preparationDescription: string;
}
@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss']
})
export class RecipeViewComponent implements OnInit {

  constructor(private fb: FormBuilder, private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  //Add typing to this form
  //https://angular.io/guide/typed-forms
  //Read about:
  //1. https://developer.mozilla.org/en-US/docs/Glossary/HTTPS
  //2. https://developer.mozilla.org/en-US/docs/Web/HTTP
  //3. https://developer.mozilla.org/en-US/docs/Glossary/REST
  //4. https://restcookbook.com/
  //5. What are most used methods and what is the usage. Which one are idempotent?
  //6. What is the diff between the PATCH and PUT and POST? 
  //6.5 Most common status codes? 200, 400, 404, 401, 204 etc.
  //7. https://angular.io/guide/http
  //8. https://angular.io/guide/architecture-services + https://angular.io/tutorial/tour-of-heroes/toh-pt4
  //9. Inject BASE_URL - https://angular.io/guide/dependency-injection + https://angular.io/guide/dependency-injection-providers#configuring-dependency-providers - just to know what is what
  //10. Observable - basic info
  //11. Each api endpoint should start with the following baseUrl + /api/v1/controller-name
  jobForm = this.fb.group({
    name: '',
    ingredients: [''],
    preperationDescription: '',
  })

  preview: string = '';
  preview2: string = '';
  ngOnInit(): void { }

  save() {
    this.preview = JSON.stringify(this.jobForm.value);
    const value = this.jobForm.value as CreateRecipe;
    this.httpClient.get(this.baseUrl + 'api/v1/WeatherForecast').subscribe(x => console.error('res', x));
    this.httpClient.post<CreateRecipe>(this.baseUrl + 'api/v1/WeatherForecast', value).subscribe(response => {
      console.error('response from request', response)
      this.preview2 = JSON.stringify(response);
    });
  }
}