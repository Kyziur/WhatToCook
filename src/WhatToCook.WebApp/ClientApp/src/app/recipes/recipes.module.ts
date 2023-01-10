import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipesComponent } from './recipes.component';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeCardComponent } from './recipe-card/recipe-card.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';


@NgModule({
  declarations: [
    RecipesComponent,
    RecipeListComponent,
    RecipeCardComponent,

    RecipeViewComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    RecipeCardComponent,
    RecipeViewComponent,
    RecipesComponent,
    RecipeListComponent,
  ],
 
})
export class RecipesModule { }
