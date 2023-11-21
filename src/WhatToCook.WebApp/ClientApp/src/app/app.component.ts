import { Component } from '@angular/core';
import { LayoutComponent } from './layout/layout.component';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [LayoutComponent],
})
export class AppComponent {}
