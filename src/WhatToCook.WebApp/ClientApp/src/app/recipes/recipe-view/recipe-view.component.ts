import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { CreateRecipe } from './CreateRecipe';
import { RecipeService } from '../recipe.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Recipe } from "../Recipe";
import { NEVER, of, switchMap } from "rxjs";
import { HttpClient } from '@angular/common/http';
export enum DisplayMode {
  New,
  Edit,
  View
}

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss']
})
export class RecipeViewComponent implements OnInit {
  timeToPrepareOptions = ["Short", "Medium", "Long"];
  recipe?: Recipe;
  selectedFile = null;
  recipeForm: FormGroup<{
    name: FormControl<string>;
    ingredients: FormArray<FormControl<string>>;
    preparationDescription: FormControl<string>;
    timeToPrepare: FormControl<string>;
    image: FormControl<string>;
  }> | null = null;
  isEditable: boolean = false;

  constructor(private fb: FormBuilder, private recipeService: RecipeService, private router: Router, private route: ActivatedRoute, private http: HttpClient) {
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
      if (!name) {
        return of(undefined);
      }
      return this.recipeService.getByName(name);
    })).subscribe(recipe => {
      this.recipe = recipe;
      this.loadFormData(recipe);
      console.error('viewing recipe:', this.recipe);
    });
  }

  getDisplayMode(): DisplayMode {
    //If there is no recipe then it means that we want to create a new one
    if (!this.recipe) {
      return DisplayMode.New;
    }

    //If there is a recipe and it is editable then it means that it is edit mode
    if (this.isEditable) {
      return DisplayMode.Edit;
    }

    return DisplayMode.View;
  }

  enableEdit() {
    this.isEditable = true;
  }

  onFileSelected(event: any) {
  const file = event.target.files[0];
  if (!file) {
    return;
  }

  const reader = new FileReader();
  reader.onload = () => {
    const image = reader.result as string;
    if (this.recipeForm) {
      this.recipeForm.get('image')?.patchValue(image.split(',')[1]);
    }
  };
  reader.readAsDataURL(file);
}
  submit() {
    const recipeload = new FormData();
    let form = this.recipeForm?.getRawValue();

    if (this.getDisplayMode() === DisplayMode.Edit && this.recipeForm) {
      const updatedRecipe = {
        id: this.recipe?.id ?? 0,
        ...this.recipeForm.getRawValue(),
      };

    if (!form)
    {
      return;
    }
    for (const [key, value] of Object.entries(form)) {
      if (Array.isArray(value)) {
        for (var v = 0 ; v < value.length; v++) {
          recipeload.append(`${key}[${v}]`, value[v]);
          console.log('Recipe load', recipeload);
        }
      } else {
        recipeload.append(key, value);
      }
    }
      this.recipeService.update(updatedRecipe).subscribe((recipe) => {

         this.recipeService.getByName(updatedRecipe.name).subscribe((recipe) => {
          this.recipe = recipe;
          this.loadFormData(this.recipe);
          this.isEditable = false;
        });
      });
    }

    if (this.getDisplayMode() === DisplayMode.New) {

      if (!form)
    {
      return;
    }
    for (const [key, value] of Object.entries(form)) {
      if (Array.isArray(value)) {
        for (var v = 0 ; v < value.length; v++) {
          recipeload.append(`${key}[${v}]`, value[v]);
        }
      } else {
        recipeload.append(key, value);
      }}
      this.recipeService.create(this.recipeForm?.value as CreateRecipe).subscribe((x) => this.handleSuccesfulSave());
    }
  }

  addIngredient() {
    this.ingredientsControls.push(this.fb.nonNullable.control(''));
  }

  loadFormData(recipe?: Recipe) {
    const ingredientsControls = recipe?.ingredients.map(ingredient => {
      return this.createStringControl(ingredient)
    }) ?? [];

    if (ingredientsControls.length === 0) {
      ingredientsControls.push(this.createStringControl(undefined))
    }

    this.recipeForm = this.fb.group({
      name: this.createStringControl(recipe?.name),
      ingredients: this.fb.array(ingredientsControls),
      preparationDescription: this.createStringControl(recipe?.preparationDescription),
      timeToPrepare: this.createStringControl(recipe?.timeToPrepare),
      image: this.createStringControl("")
    })
  }
  //return current time
  createStringControl(value: string | undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }
}
