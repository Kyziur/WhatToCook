import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LayoutComponent } from './layout.component';
import { Routes, RouterModule, Router } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { MenuListComponent } from './sidebar/menu-list/menu-list.component';
import { ProfileComponent } from './profile/profile.component';
import { SharedComponent } from '../shared/shared.component';
import { SharedModule } from '../shared/shared.module';
import { MealPlanningComponent } from './sidebar/meal-planning/meal-planning.component';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { PlanSelectComponent } from '../shared/plan-select/plan-select.component';

const routes: Routes = [
    {
      path:'meal-plan',
      component: MealPlanningComponent, 
    },
    {
        path: 'recipes/:name',
        component: MealPlanningComponent,
    },
    
  ]
@NgModule({
    declarations: [
        LayoutComponent,
        SidebarComponent,
        MenuListComponent,
        MealPlanningComponent,
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routes),
        ReactiveFormsModule,
        HttpClientModule,  
    ],
    exports: [
        LayoutComponent,
        ReactiveFormsModule,
        RouterModule,
    ]
})
export class LayoutModule { }
