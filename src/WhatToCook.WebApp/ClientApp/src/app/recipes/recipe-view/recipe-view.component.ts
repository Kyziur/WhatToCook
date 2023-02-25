import {HttpClient} from '@angular/common/http';
import {Component, Inject, OnInit, Type} from '@angular/core';
import {FormControl, FormGroup, FormBuilder, FormArray} from '@angular/forms';
import {ReactiveFormsModule} from '@angular/forms';
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

  constructor(private fb: FormBuilder, private recipeService: RecipeService, private router: Router, private route: ActivatedRoute,) {
  }

  handleSuccesfulSave() {
    this.router.navigate(['recipes'])
  }

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
  preview3: string = '';

  ngOnInit(): void {
    //TODO: Add check if it is create or edit
    //params examples:
    //query param: https://fancypage.com/recipe?name="pierogi"
    //route param: https://fancypage.com/recipe/pierogi

    this.route.params.pipe(switchMap(params => {
      const name = params['name']
      if(!name){
        return NEVER;
      }
      return this.recipeService.getByName(name);
    })).subscribe(recipe => {
      this.recipe = recipe;
      console.error('viewing recipe:', this.recipe);
    });
  }

  save() {
    this.recipeService.create(this.recipeForm.value as CreateRecipe).subscribe(x => this.handleSuccesfulSave())
  }


  addIngredient() {
    this.ingredientsControls.push(this.fb.nonNullable.control(''));
  }
}
