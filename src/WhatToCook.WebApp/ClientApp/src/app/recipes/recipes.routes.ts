import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeViewComponent } from './recipe-view/recipe-view.component';
import { Routes } from '@angular/router';

export const RECIPES_ROUTES: Routes = [
  {
    path: 'recipes',
    children: [
      {
        path: '',
        component: RecipeListComponent,
      },
      {
        path: 'new',
        component: RecipeViewComponent,
      },
      {
        path: ':name',
        component: RecipeViewComponent,
      },
    ],
  },
];
