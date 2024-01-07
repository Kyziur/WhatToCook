import { enableProdMode, importProvidersFrom } from '@angular/core';

import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { provideRouter, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  withInterceptorsFromDi,
  provideHttpClient,
} from '@angular/common/http';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { MEAL_PLANNER_ROUTES } from './app/meal-planner/mealPlanner.routes';
import { RECIPES_ROUTES } from './app/recipes/recipes.routes';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'recipes',
    pathMatch: 'full',
  },
  ...RECIPES_ROUTES,
  ...MEAL_PLANNER_ROUTES,
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
      FormsModule
    ),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(routes),
  ],
}).catch(err => console.log(err));
