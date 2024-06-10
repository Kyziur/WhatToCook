import { Component, Input } from '@angular/core';
import { NgIf, NgSwitch, NgSwitchCase } from '@angular/common';

export interface Badge {
  level: 'info' | 'success' | 'warning' | 'error';
  text: string;
}

@Component({
  selector: 'app-badge',
  templateUrl: './badge.component.html',
  standalone: true,
  imports: [NgIf, NgSwitch, NgSwitchCase],
})
export class BadgeComponent {
  @Input() badge?: Badge;
}
