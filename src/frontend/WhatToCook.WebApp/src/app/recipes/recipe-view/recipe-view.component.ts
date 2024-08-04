import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { switchMap, of, tap } from 'rxjs';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { TextareaAutoResizeDirective } from '../../shared/directives/textarea-auto-resize/textarea-auto-resize.directive';
import { Recipe, EMPTY_RECIPE, CreateRecipe } from '../recipe.types';
import { RecipeFormComponent } from '../recipe-form/recipe-form.component';
import { RecipeService } from '../recipe.service';

export enum DisplayMode {
  New = 'New',
  Edit = 'Edit',
  View = 'View',
}

@Component({
  selector: 'app-recipe-view',
  templateUrl: './recipe-view.component.html',
  standalone: true,
  imports: [
    CommonModule,
    ModalComponent,
    ReactiveFormsModule,
    FormsModule,
    TextareaAutoResizeDirective,
    RecipeFormComponent,
  ],
})
export class RecipeViewComponent implements OnInit {
  recipe: Recipe = EMPTY_RECIPE;
  displayMode = DisplayMode.View;
  isDeleteConfirmationVisible = false;

  constructor(
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
        switchMap(params => {
          const name = params['name'];
          return name ? this.recipeService.getByName(name) : of(undefined);
        }),
        tap(recipe => {
          if (!recipe) {
            this.enableCreation();
            return;
          }

          this.recipe = recipe;
        })
      )
      .subscribe();
  }

  enableCreation() {
    this.displayMode = DisplayMode.New;
  }

  enableEdit() {
    this.displayMode = DisplayMode.Edit;
  }

  disableEdit() {
    this.displayMode = DisplayMode.View;
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

  submit(recipe: CreateRecipe) {
    if (!recipe) {
      const msg = 'Cannot update recipe that is undefined';
      console.error(msg, recipe);
      throw new Error(msg);
    }

    switch (this.displayMode) {
      case DisplayMode.New:
        this.recipeService
          .create(recipe)
          .subscribe(() => this.redirectToRecipesPage());
        break;
      case DisplayMode.Edit:
        this.recipeService
          .update(recipe)
          .pipe(
            switchMap(() => this.recipeService.getByName(recipe.name)),
            tap(value => {
              this.recipe = value;
              this.disableEdit();
            })
          )
          .subscribe();
        break;
      case DisplayMode.View:
        break;
    }
  }
}
