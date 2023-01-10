import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedComponent } from '../shared/shared.component';
import { FavouritesButtonComponent } from './favourites-button/favourites-button.component';

@NgModule({
  declarations: [
    SharedComponent,
    FavouritesButtonComponent
  ],
  imports: [
    CommonModule
  ]
})
export class SharedModule { }
