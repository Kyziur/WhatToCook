import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {LayoutComponent} from './layout.component';
import {RouterModule, Routes} from '@angular/router';
import {SidebarComponent} from './sidebar/sidebar.component';
import {MenuListComponent} from './sidebar/menu-list/menu-list.component';
import {SharedModule} from '../shared/shared.module';
import {ReactiveFormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';

@NgModule({
  declarations: [
    LayoutComponent,
    SidebarComponent,
    MenuListComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    RouterModule,
    HttpClientModule,
  ],
  exports: [
    LayoutComponent,
    ReactiveFormsModule,
    RouterModule,
  ]
})
export class LayoutModule {
}
