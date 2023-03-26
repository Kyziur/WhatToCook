import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {LayoutComponent} from './layout.component';
import {RouterModule} from '@angular/router';
import {SidebarComponent} from './sidebar/sidebar.component';
import {MenuListComponent} from './sidebar/menu-list/menu-list.component';


@NgModule({
  declarations: [
    LayoutComponent,
    SidebarComponent,
    MenuListComponent,

  ],
  imports: [
    CommonModule,
    RouterModule,
  ],
  exports: [
    LayoutComponent
  ]
})
export class LayoutModule {
}
