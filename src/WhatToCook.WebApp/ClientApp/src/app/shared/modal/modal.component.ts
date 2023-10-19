import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class ModalComponent {
  @Input() public visible = false;
  @Output() public visibleChange = new EventEmitter<boolean>();

  closeClickHandler() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }
}
