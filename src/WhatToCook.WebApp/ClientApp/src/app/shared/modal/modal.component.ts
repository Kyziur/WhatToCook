import { Component, Input } from '@angular/core';

export interface Button {
  isVisible: boolean;
  text: string;
}

export const EMPTY_BUTTON: Button = {
  text: "",
  isVisible: false
}

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {

  @Input() public isVisible = false;
  @Input() public header = "";

  @Input() public cancelButton = EMPTY_BUTTON;
  @Input() public acceptButton = EMPTY_BUTTON;
  constructor() {
  }
}
