import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipesComponent } from './recipes.component';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeCardComponent } from './recipe-card/recipe-card.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';
import { SharedModule } from '../shared/shared.module';
import { SearchComponent } from '../shared/search/search.component';

@NgModule({
  declarations: [
    RecipesComponent,
    RecipeListComponent,
    RecipeCardComponent,
    RecipeViewComponent,
  ],
  imports: [
    CommonModule,
    SharedModule
  ],
  exports: [
    RecipeCardComponent,
    RecipeViewComponent,
    RecipesComponent,
    RecipeListComponent,
  ],
 
})
export class RecipesModule { }
