import { BrowserModule } from '@angular/platform-browser';
import { Component, NgModule, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Routes, RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { LayoutModule } from './layout/layout.module';
import { RecipeListComponent } from './recipes/recipe-list/recipe-list.component';
import { RecipeViewComponent } from './recipes/recipe-view/recipe-view.component';
import { RecipeCardComponent } from './recipes/recipe-card/recipe-card.component';
import { RecipesModule } from './recipes/recipes.module';
import { MenuListComponent } from './layout/sidebar/menu-list/menu-list.component';
import { CreateRecipe } from './recipes/recipe-view/CreateRecipe';
import {MealPlannerModule} from "./meal-planner/meal-planner.module";


const routes: Routes = [

  {
    path: '',
    redirectTo: 'recipes',
    pathMatch: 'full'
  },

];

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(routes),
    LayoutModule,
    RecipesModule,
    MealPlannerModule

  ],
  providers: [],
  bootstrap: [AppComponent],
  exports: [RouterModule]
})
export class AppModule { }
