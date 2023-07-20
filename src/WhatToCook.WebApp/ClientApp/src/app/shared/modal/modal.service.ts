import {ComponentRef, Injectable, Type, ViewContainerRef} from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  private _isVisible = false;
  private viewContainerRef?: ViewContainerRef;
  constructor() {

  }

  showModal(component: Type<unknown>, viewContainerRef: ViewContainerRef){
    this.viewContainerRef = viewContainerRef;

    this.viewContainerRef.createComponent(component);
    this._isVisible = true;
  }

  closeModal(){
    this._isVisible = false;
  }

  isVisible() {
    return this._isVisible;
  }

}
