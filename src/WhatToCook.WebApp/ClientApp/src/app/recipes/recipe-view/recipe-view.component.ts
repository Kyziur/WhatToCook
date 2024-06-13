import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CreateRecipe } from './CreateRecipe';
import { RecipeService } from '../recipe.service';
import { ActivatedRoute, Router } from '@angular/router';
import { EMPTY_RECIPE, Recipe } from '../Recipe';
import { of, switchMap, tap } from 'rxjs';
import { ModalComponent } from '../../shared/modal/modal.component';
import {
  NgSwitch,
  NgSwitchCase,
  NgTemplateOutlet,
  NgIf,
  NgFor,
} from '@angular/common';
import { TextareaAutoResizeDirective } from 'src/app/shared/textarea-auto-resize/textarea-auto-resize.directive';
import { RecipeFormComponent } from '../recipe-form/recipe-form.component';

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
    NgSwitch,
    NgSwitchCase,
    NgTemplateOutlet,
    NgIf,
    ModalComponent,
    NgFor,
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
