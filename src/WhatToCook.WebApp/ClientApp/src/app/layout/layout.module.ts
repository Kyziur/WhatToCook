import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LayoutComponent } from './layout.component';
import { RouterModule } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { MenuListComponent } from './sidebar/menu-list/menu-list.component';
import { ProfileComponent } from './profile/profile.component';
import { SharedComponent } from '../shared/shared.component';
import { SharedModule } from '../shared/shared.module';


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
export class LayoutModule { }
