import { NgModule } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeCardComponent } from './recipe-card/recipe-card.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';

import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RecipeListService } from './recipe-list/recipe-list.service';

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
  },
];

@NgModule({
    imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    HttpClientModule,
    FormsModule,
    NgOptimizedImage,
    RecipeListComponent, RecipeCardComponent, RecipeViewComponent,
],
    exports: [
        RecipeCardComponent,
        RecipeViewComponent,
        RecipeListComponent,
        RouterModule,
        ReactiveFormsModule,
    ],
    providers: [RecipeListService],
})
export class RecipesModule {}
