import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { LayoutModule } from './layout/layout.module';
import { RecipeListComponent } from './recipes/recipe-list/recipe-list.component';
import { RecipeViewComponent } from './recipes/recipe-view/recipe-view.component';
import { RecipeCardComponent } from './recipes/recipe-card/recipe-card.component';
import { RecipesModule } from './recipes/recipes.module';
import { RecipesComponent } from './recipes/recipes.component';
@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
     // { path: 'appcomponent', component: AppComponent},
      //{ path: '', redirectTo: "appcomponent", pathMatch: 'full'},
      { path: 'recipes', component: RecipeListComponent},
      { path: '', redirectTo: "recipes", pathMatch: 'full'},
      { path: 'recipe', component: RecipeViewComponent},
      

    ]),
    LayoutModule
  ],
  providers: [],
  bootstrap: [AppComponent]
  
})
export class AppModule { }