import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipesComponent } from './recipes.component';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeCardComponent } from './recipe-card/recipe-card.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';
import { SharedModule } from '../shared/shared.module';
import { SearchComponent } from '../shared/search/search.component';
import { Routes, RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

const routes: Routes = [
  {
    path: 'recipes',
    component: RecipeListComponent,
  },
  {
    path: 'recipes/new',
    component: RecipeViewComponent,
  },
  {
    path: 'recipes/:name',
    component: RecipeViewComponent,
  }
];

@NgModule({
  declarations: [
    RecipesComponent,
    RecipeListComponent,
    RecipeCardComponent,
    RecipeViewComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    HttpClientModule
  ],
  exports: [
    RecipeCardComponent,
    RecipeViewComponent,
    RecipesComponent,
    RecipeListComponent,
    RouterModule,
  ],

})
export class RecipesModule { }
