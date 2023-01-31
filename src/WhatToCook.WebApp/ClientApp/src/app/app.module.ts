import { BrowserModule } from '@angular/platform-browser';
import { Component, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { Routes, RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { LayoutModule } from './layout/layout.module';
import { RecipeListComponent } from './recipes/recipe-list/recipe-list.component';
import { RecipeViewComponent } from './recipes/recipe-view/recipe-view.component';
import { RecipeCardComponent } from './recipes/recipe-card/recipe-card.component';
import { RecipesModule } from './recipes/recipes.module';
import { RecipesComponent } from './recipes/recipes.component';
import { MenuListComponent } from './layout/sidebar/menu-list/menu-list.component';



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
   
  ],
  providers: [],
  bootstrap: [AppComponent],
  exports: [RouterModule]
})
export class AppModule { }
