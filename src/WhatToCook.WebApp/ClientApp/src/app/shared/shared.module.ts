import { NgModule } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { SharedComponent } from './shared.component';
import { FavouritesButtonComponent } from './favourites-button/favourites-button.component';
import { SearchComponent } from './search/search.component';
import { ModalComponent } from './modal/modal.component';
import { BadgeComponent } from './badge/badge.component';

@NgModule({
  declarations: [
    SharedComponent,
    FavouritesButtonComponent,
    SearchComponent,
    ModalComponent,
    BadgeComponent,
  ],
  imports: [CommonModule, NgOptimizedImage],
  exports: [
    FavouritesButtonComponent,
    SearchComponent,
    ModalComponent,
    BadgeComponent,
  ],
})
export class SharedModule {}
