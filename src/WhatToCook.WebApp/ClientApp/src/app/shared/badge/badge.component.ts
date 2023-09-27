import {Component, Input} from '@angular/core';


export interface Badge {
  level: "info" | "success" | "warning" | "error",
  text: string
}

@Component({
  selector: 'app-badge',
  templateUrl: './badge.component.html',
  styleUrls: ['./badge.component.scss']
})
export class BadgeComponent {
  @Input() badge?: Badge
}
