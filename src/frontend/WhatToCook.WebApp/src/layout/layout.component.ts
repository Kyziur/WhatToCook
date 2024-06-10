import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

interface MenuItem {
  title: string;
  link: string;
}
@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
})
export class LayoutComponent {
  menuItems: MenuItem[] = [
    {
      title: 'Recipes',
      link: '/recipes',
    },
    {
      title: 'Create recipe',
      link: '/recipes/new',
    },
    {
      title: 'Meal plans',
      link: '/meal-plans',
    },
    {
      title: 'Create meal plan',
      link: '/meal-plans/new',
    },
  ];
}
