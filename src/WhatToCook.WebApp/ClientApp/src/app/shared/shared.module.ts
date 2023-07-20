import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from '../shared/shared.component';
import { FavouritesButtonComponent } from './favourites-button/favourites-button.component';
import { SearchComponent } from './search/search.component';
import { PlanSelectComponent } from './plan-select/plan-select.component';



@NgModule({
  declarations: [
    SharedComponent,
    FavouritesButtonComponent,
    SearchComponent,
    PlanSelectComponent,
    
  ],
  imports: [
    CommonModule
  ],
exports: [FavouritesButtonComponent, SearchComponent, PlanSelectComponent],
})
export class SharedModule { }
