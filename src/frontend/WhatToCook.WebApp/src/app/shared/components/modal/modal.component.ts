import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgIf, NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  standalone: true,
  imports: [NgIf, NgOptimizedImage],
})
export class ModalComponent {
  @Input() public title = '';
  @Input() public visible = false;
  @Output() public visibleChange = new EventEmitter<boolean>();

  closeClickHandler() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }
}
