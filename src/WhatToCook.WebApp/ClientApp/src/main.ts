import { enableProdMode, importProvidersFrom } from '@angular/core';

import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { MealPlannerModule } from './app/meal-planner/meal-planner.module';
import { RecipesModule } from './app/recipes/recipes.module';
import { provideRouter, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  withInterceptorsFromDi,
  provideHttpClient,
} from '@angular/common/http';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'recipes',
    pathMatch: 'full',
  },
];

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }];

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    ...providers,
    importProvidersFrom(
      BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
      FormsModule,
      RecipesModule,
      MealPlannerModule
    ),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(routes),
  ],
}).catch(err => console.log(err));
