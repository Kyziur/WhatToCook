import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, FormArray } from '@angular/forms';
import { CreateRecipe } from './CreateRecipe';
import { RecipeService } from '../recipe.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { of, switchMap } from 'rxjs';
import {
  mapTimeToPrepareToBadge,
  TimeToPrepare,
  TimeToPrepareValues,
} from '../TimeToPrepare';

export enum DisplayMode {
  New,
  Edit,
  View,
}

export interface RecipeForm {
  name: FormControl<string>;
  ingredients: FormArray<FormControl<string>>;
  preparationDescription: FormControl<string>;
  timeToPrepare: FormControl<TimeToPrepare>;
  image: FormControl<string>;
}

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  styleUrls: ['./recipe-view.component.scss'],
})
export class RecipeViewComponent implements OnInit {
  protected readonly TimeToPrepareValues = TimeToPrepareValues;
  isDeleteConfirmationVisible = false;
  recipe?: Recipe;
  recipeForm: FormGroup<RecipeForm> | null = null;
  isEditable: boolean = false;

  constructor(
    private fb: FormBuilder,
    private recipeService: RecipeService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  redirectToRecipesPage() {
    this.router.navigate(['recipes']);
  }

  mapPrepareTimeToBadge(recipe: Recipe) {
    return mapTimeToPrepareToBadge(recipe.timeToPrepare);
  }

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap(params => {
          const name = params['name'];
          if (!name) {
            return of(undefined);
          }
          return this.recipeService.getByName(name);
        })
      )
      .subscribe(recipe => {
        this.recipe = recipe;
        this.loadFormData(recipe);
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

  disableEdit() {
    this.isEditable = false;
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

  private updateRecipe(form: FormGroup<RecipeForm>) {
    const updatedRecipe: CreateRecipe = {
      id: this.recipe?.id ?? 0,
      ...form.getRawValue(),
    };

    return this.recipeService.update(updatedRecipe).pipe(
      switchMap(() => {
        return this.recipeService.getByName(updatedRecipe.name);
      })
    );
  }

  submit() {
    if (!this.recipeForm) {
      const msg = 'Cannot update recipe that is undefined';
      console.error(msg, this.recipeForm);
      throw new Error(msg);
    }

    switch (this.getDisplayMode()) {
      case DisplayMode.New:
        this.recipeService
          .create(this.recipeForm.value as CreateRecipe)
          .subscribe(() => this.redirectToRecipesPage());
        break;
      case DisplayMode.Edit:
        this.updateRecipe(this.recipeForm).subscribe(recipe => {
          this.recipe = recipe;
          this.loadFormData(this.recipe);
          this.isEditable = false;
        });
        break;
      case DisplayMode.View:
        break;
    }
  }

  addIngredient() {
    this.recipeForm?.controls.ingredients.push(this.fb.nonNullable.control(''));
  }

  loadFormData(recipe?: Recipe) {
    const ingredientsControls =
      recipe?.ingredients.map(ingredient => {
        return this.createStringControl(ingredient);
      }) ?? [];

    if (ingredientsControls.length === 0) {
      ingredientsControls.push(this.createStringControl());
    }

    this.recipeForm = this.fb.group({
      name: this.createStringControl(recipe?.name),
      ingredients: this.fb.array(ingredientsControls),
      preparationDescription: this.createStringControl(
        recipe?.preparationDescription
      ),
      timeToPrepare: this.fb.nonNullable.control<TimeToPrepare>(
        recipe?.timeToPrepare ?? 'Short'
      ),
      image: this.createStringControl(),
    });
  }

  createStringControl(value: string | undefined = undefined) {
    return this.fb.nonNullable.control(value ?? '');
  }

  getImagePath() {
    return this.recipe ? this.recipe.imagePath : '';
  }

  openDeleteConfirmation() {
    this.isDeleteConfirmationVisible = true;
  }

  closeDeleteConfirmation() {
    this.isDeleteConfirmationVisible = false;
  }

  onDeleteClickHandler(id?: number) {
    if (id === undefined) {
      console.error('Cannot remove recipe because it was not saved');
      return;
    }

    this.recipeService.deleteRecipe(id).subscribe({
      next: () => this.redirectToRecipesPage(),
      error: error =>
        console.error('Error occured when deleting recipe', error),
    });
  }

  setDefaultImage() {
    if (this.recipe) {
      this.recipe.imagePath = 'Images/default_image.png';
    }
  }

  cancelEditClickHandler() {
    this.disableEdit();
  }
}
