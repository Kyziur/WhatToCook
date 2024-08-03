import { CommonModule, NgOptimizedImage } from "@angular/common";
import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from "@angular/core";
import { Router } from "@angular/router";
import { BadgeComponent } from "../../shared/badge/badge.component";
import { PrepareTimeToBadgePipe } from "../prepare-time-to-badge.pipe";
import { Recipe } from "../Recipe";


export interface RecipeCard extends Recipe {
  isSelected: boolean;
}

@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  standalone: true,
  imports: [
    CommonModule,
    BadgeComponent,
    NgOptimizedImage,
    PrepareTimeToBadgePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipeCardComponent {
  @Input() recipe?: RecipeCard;
  @Input() showSelectButton = false;

  @Output() selectClicked = new EventEmitter<Pick<RecipeCard, 'id'>>();
  constructor(private router: Router) {}

  viewRecipeDetails(name: string | undefined) {
    if (name === undefined) {
      return;
    }

    this.router.navigate([`/recipes/${name}`]);
  }

  get imagePath() {
    return this.recipe?.imagePath ?? 'Images/default_image.png';
  }

  selectClickHandler() {
    if (this.recipe) {
      this.selectClicked.emit({
        id: this.recipe.id,
      });
    }
  }
}
