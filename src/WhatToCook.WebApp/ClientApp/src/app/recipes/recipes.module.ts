import { NgModule} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeCardComponent } from './recipe-card/recipe-card.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';
import { SharedModule } from '../shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
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
  },

];

@NgModule({
  declarations: [
    RecipeListComponent,
    RecipeCardComponent,
    RecipeViewComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    HttpClientModule,
    FormsModule,
  ],
  exports: [
    RecipeCardComponent,
    RecipeViewComponent,
    RecipeListComponent,
    RouterModule,
    ReactiveFormsModule,
  ],

})
export class RecipesModule { }
