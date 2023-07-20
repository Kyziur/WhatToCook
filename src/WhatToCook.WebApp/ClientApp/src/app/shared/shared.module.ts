import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from './shared.component';
import { FavouritesButtonComponent } from './favourites-button/favourites-button.component';
import { SearchComponent } from './search/search.component';
import { ModalComponent } from './modal/modal.component';

@NgModule({
  declarations: [
    SharedComponent,
    FavouritesButtonComponent,
    SearchComponent,
    ModalComponent
  ],
  imports: [
    CommonModule
  ],
exports: [FavouritesButtonComponent, SearchComponent],
})
export class SharedModule { }
