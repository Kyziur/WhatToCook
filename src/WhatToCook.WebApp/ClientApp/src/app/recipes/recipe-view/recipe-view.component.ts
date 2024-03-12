import { Component, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormBuilder,
  FormArray,
  ReactiveFormsModule,
  FormsModule,
} from '@angular/forms';
import { CreateRecipe } from './CreateRecipe';
import { RecipeService } from '../recipe.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Recipe } from '../Recipe';
import { of, switchMap } from 'rxjs';
import { TimeToPrepare, TimeToPrepareValues } from '../TimeToPrepare';
import { ModalComponent } from '../../shared/modal/modal.component';
import {
  NgSwitch,
  NgSwitchCase,
  NgTemplateOutlet,
  NgIf,
  NgFor,
} from '@angular/common';
import { TextareaAutoResizeDirective } from 'src/app/shared/textarea-auto-resize/textarea-auto-resize.directive';

export enum DisplayMode {
  New = 'New',
  Edit = 'Edit',
  View = 'View',
}

export interface RecipeForm {
  name: FormControl<string>;
  ingredients: FormArray<FormControl<string>>;
  preparationDescription: FormControl<string>;
  timeToPrepare: FormControl<TimeToPrepare>;
  image: FormControl<string>;
  tags: FormControl<string>;
}

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  standalone: true,
  imports: [
    NgSwitch,
    NgSwitchCase,
    NgTemplateOutlet,
    NgIf,
    ModalComponent,
    NgFor,
    ReactiveFormsModule,
    FormsModule,
    TextareaAutoResizeDirective,
  ],
})
export class RecipeViewComponent implements OnInit {
  protected readonly TimeToPrepareValues = TimeToPrepareValues;
  isDeleteConfirmationVisible = false;
  recipe?: Recipe;
  recipeForm: FormGroup<RecipeForm> | null = null;
  isEditable = false;

  constructor(
    private fb: FormBuilder,
    private recipeService: RecipeService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  redirectToRecipesPage() {
    this.router.navigate(['recipes']).then();
  }

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap((params) => {
          const name = params['name'];
          if (!name) {
            return of(undefined);
          }
          return this.recipeService.getByName(name);
        })
      )
      .subscribe((recipe) => {
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

  onFileSelected($event: Event) {
    const target = $event.target as HTMLInputElement;

    if (!target || !target.files?.length) {
      return;
    }

    const file = target.files[0];
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
      tags: form.controls.tags.value.split(',').map((x) => x.trim()),
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
          .create({
            ...this.recipeForm.value,
            tags: this.recipeForm.value.tags?.split(',').map((x) => x.trim()),
          } as CreateRecipe)
          .subscribe(() => this.redirectToRecipesPage());
        break;
      case DisplayMode.Edit:
        this.updateRecipe(this.recipeForm).subscribe((recipe) => {
          this.recipe = recipe;
          this.loadFormData(this.recipe);
          this.isEditable = false;
        });
        break;
      case DisplayMode.View:
        break;
    }
  }

  isDialogToShowIngredientsVisible = false;
  ingredientsToParse = '';
  openDialogToPasteIngredients() {
    this.isDialogToShowIngredientsVisible = true;
  }
  closeDialogToPasteIngredients() {
    this.isDialogToShowIngredientsVisible = false;
  }

  parseIngredients() {
    const splitted = this.ingredientsToParse
      .trim()
      .split(/\r?\n/)
      .filter((x) => x.length > 0);

    if (splitted.length < 1) {
      return;
    }

    splitted.forEach((ingredient) => this.addIngredient(ingredient));

    this.removeEmptyIngredients();
    this.ingredientsToParse = '';
    this.closeDialogToPasteIngredients();
  }

  addIngredient(value: string = '') {
    this.recipeForm?.controls.ingredients.push(
      this.createStringControl(value.trim())
    );
  }

  removeEmptyIngredients() {
    const indexesOfEmptyFields = this.recipeForm?.controls.ingredients.controls
      .map((field, index) => (field.value.trim().length === 0 ? index : -1))
      .filter((x) => x > -1);

    indexesOfEmptyFields?.forEach((x) => this.removeIngredient(x));
  }

  removeIngredient(index: number) {
    if (!this.recipeForm) {
      return;
    }

    this.recipeForm.controls.ingredients.removeAt(index);
  }

  loadFormData(recipe?: Recipe) {
    const ingredientsControls =
      recipe?.ingredients.map((ingredient) => {
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
      tags: this.createStringControl(recipe?.tags.join(',')),
    });
  }

  createStringControl(value: string | undefined = undefined) {
    return this.fb.nonNullable.control(value ?? '');
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
      error: (error) =>
        console.error('Error occured when deleting recipe', error),
    });
  }

  cancelEditClickHandler() {
    this.disableEdit();
  }
}
