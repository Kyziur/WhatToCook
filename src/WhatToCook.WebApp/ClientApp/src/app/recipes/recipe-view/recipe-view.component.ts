import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss']
})
export class RecipeViewComponent implements OnInit{

  constructor(private fb: FormBuilder) {}
 
  jobForm = this.fb.group({
    name: [''],
    ingredients: [''],
    preperationDescription:[''],
  })
 
  preview: string = '';
 
  ngOnInit(): void {}
 
  save() {
    this.preview = JSON.stringify(this.jobForm.value);
  }

}