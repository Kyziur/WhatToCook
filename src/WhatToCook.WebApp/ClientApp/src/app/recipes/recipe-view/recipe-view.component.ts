import {Component, OnInit} from '@angular/core';
import {FormControl, FormGroup, FormBuilder, FormArray} from '@angular/forms';
import {CreateRecipe} from './CreateRecipe';
import {RecipeService} from '../recipe.service';
import {ActivatedRoute, Router} from '@angular/router';
import {Recipe} from "../Recipe";
import {NEVER, of, switchMap} from "rxjs";

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss']
})
export class RecipeViewComponent implements OnInit {
  timeToPrepareOptions = ["Short", "Medium", "Long"];
  recipe?: Recipe;
  recipeForm: FormGroup | null = null;
  isEditable: boolean = false;

  constructor(private fb: FormBuilder, private recipeService: RecipeService, private router: Router, private route: ActivatedRoute) {
  }

  handleSuccesfulSave() {
    this.router.navigate(['recipes'])
  }

  get ingredientsControls(): FormArray<FormControl<string>> {
    return this.recipeForm?.get('ingredients') as FormArray<FormControl<string>>;
  }

  preview: string = '';
  preview2: string = '';
  preview3: string = '';

  ngOnInit(): void {
    //TODO: Add check if it is create or edit
    //params examples:
    //query param: https://fancypage.com/recipe?name="pierogi"
    //route param: https://fancypage.com/recipe/pierogi
    //route param: https://fancypage.com/recipe/create

    this.route.params.pipe(switchMap(params => {
      console.error('params', params);
      const name = params['name']
      if(!name){
        return of(undefined);
      }
      return this.recipeService.getByName(name);
    })).subscribe(recipe => {
      this.recipe = recipe;
      this.loadFormData(recipe);
      console.error('viewing recipe:', this.recipe);
    });
  }

  enableEdit() {
    this.isEditable = true;
  }
  save(){
    //TODO: ADD REQUEST TO API TO UPDATE EDITED RECIPE
    //AFTER SAVE RELOAD DATA
    this.isEditable = false;
  }

  submit() {
    this.recipeService.create(this.recipeForm?.value as CreateRecipe).subscribe(x => this.handleSuccesfulSave())
  }

  addIngredient() {
    this.ingredientsControls.push(this.fb.nonNullable.control(''));
  }

  loadFormData(recipe?: Recipe) {
    const ingredientsControls = recipe?.ingredients.map(ingredient => {
      return this.createStringControl(ingredient)
    }) ?? [];

    if(ingredientsControls.length === 0){
      ingredientsControls.push(this.createStringControl(undefined))
    }

    this.recipeForm = this.fb.group({
      name: this.createStringControl(recipe?.name),
      ingredients: this.fb.array(ingredientsControls),
      preparationDescription: this.createStringControl(recipe?.preparationDescription),
      timeToPrepare: this.createStringControl(recipe?.timeToPrepare)
    })
  }

  createStringControl(value: string | undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }
}
