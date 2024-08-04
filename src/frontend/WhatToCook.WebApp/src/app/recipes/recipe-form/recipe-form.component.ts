import { NgIf } from '@angular/common';
import {
  Component,
  ChangeDetectionStrategy,
  input,
  signal,
  Output,
  EventEmitter,
  effect,
} from '@angular/core';
import {
  FormControl,
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  FormGroup,
  FormBuilder,
} from '@angular/forms';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { TextareaAutoResizeDirective } from '../../shared/directives/textarea-auto-resize/textarea-auto-resize.directive';
import {
  TimeToPrepare,
  TimeToPrepareValues,
} from '../prepare-time-to-badge.pipe';
import { CreateRecipe, Recipe } from '../recipe.types';

export interface RecipeForm {
  name: FormControl<string>;
  ingredients: FormArray<FormControl<string>>;
  preparationDescription: FormControl<string>;
  timeToPrepare: FormControl<TimeToPrepare>;
  image: FormControl<string>;
  tags: FormControl<string>;
}

@Component({
  selector: 'app-recipe-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgIf,
    FormsModule,
    ReactiveFormsModule,
    ModalComponent,
    TextareaAutoResizeDirective,
  ],
  templateUrl: './recipe-form.component.html',
  styleUrl: './recipe-form.component.scss',
})
export class RecipeFormComponent {
  recipe = input.required<Recipe>();
  image = signal<string>('');
  readonly TimeToPrepareValues = TimeToPrepareValues;

  @Output() submitted = new EventEmitter<CreateRecipe>();
  @Output() goBackClicked = new EventEmitter<void>();

  recipeForm: FormGroup<RecipeForm> | null = null;
  isDialogToShowIngredientsVisible = false;
  ingredientsToParse = '';

  constructor(private fb: FormBuilder) {
    effect(
      () => {
        this.loadFormData(this.recipe());
        this.image.set(this.recipe().imagePath);
      },
      {
        allowSignalWrites: true,
      }
    );
  }

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
      .filter(x => x.length > 0);

    if (splitted.length < 1) {
      return;
    }

    splitted.forEach(ingredient => this.addIngredient(ingredient));

    this.removeEmptyIngredients();
    this.ingredientsToParse = '';
    this.closeDialogToPasteIngredients();
  }

  addIngredient(value = '') {
    this.recipeForm?.controls.ingredients.push(
      this.createStringControl(value.trim())
    );
  }

  removeEmptyIngredients() {
    const indexesOfEmptyFields = this.recipeForm?.controls.ingredients.controls
      .map((field, index) => (field.value.trim().length === 0 ? index : -1))
      .filter(x => x > -1);

    indexesOfEmptyFields?.forEach(x => this.removeIngredient(x));
  }

  removeIngredient(index: number) {
    if (!this.recipeForm) {
      return;
    }

    this.recipeForm.controls.ingredients.removeAt(index);
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
      tags: this.createStringControl(recipe?.tags.join(',')),
    });
  }

  createStringControl(value: string | undefined = undefined) {
    return this.fb.nonNullable.control(value ?? '');
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
        this.recipeForm.controls.image.patchValue(image.split(',')[1]);
        this.image.set(image);
      }
    };
    reader.readAsDataURL(file);
  }

  submitClicked() {
    const formValue = this.recipeForm?.value;
    if (!formValue) {
      return;
    }

    const recipe: CreateRecipe = {
      ...formValue,
      tags: (formValue.tags ?? '').split(',').map(x => x.trim()),
      id: this.recipe().id,
    } as CreateRecipe;

    this.submitted.emit(recipe);
  }
}
