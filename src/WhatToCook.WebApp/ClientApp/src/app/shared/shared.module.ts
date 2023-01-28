import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from '../shared/shared.component';
import { FavouritesButtonComponent } from './favourites-button/favourites-button.component';
import { SearchComponent } from './search/search.component';

@NgModule({
  declarations: [
    SharedComponent,
    FavouritesButtonComponent,
    SearchComponent,
    
  ],
  imports: [
    CommonModule
  ],
exports: [FavouritesButtonComponent, SearchComponent],
})
export class SharedModule { }
