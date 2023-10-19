import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { RecipeCard } from '../recipe-card/recipe-card.component';
import { RecipeService } from '../recipe.service';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss'],
})
export class RecipeListComponent implements OnInit {
  recipeCards: RecipeCard[] = [];
  @Input() allowSelection = false;
  @Input() selectedRecipeCards: RecipeCard[] = [];
  @Output() selectedRecipeCardsChange = new EventEmitter<RecipeCard[]>();

  constructor(public service: RecipeService) {}

  ngOnInit(): void {
    this.service.get().subscribe({
      next: recipes => {
        this.recipeCards = recipes.map(recipe => ({
          ...recipe,
          isSelected: false,
        }));
      },
    });
  }

  recipeCardSelectionChange($event: RecipeCard) {
    if (!this.allowSelection) {
      return;
    }

    const selectionIndex = this.findIndexInSelected($event);

    if (selectionIndex === -1) {
      this.selectedRecipeCards.push($event);
    } else {
      this.selectedRecipeCards = this.selectedRecipeCards.filter(
        r => r.id !== $event.id
      );
    }

    this.selectedRecipeCardsChange.emit(this.selectedRecipeCards);
  }

  isRecipeCardSelected(recipeCard: RecipeCard) {
    return this.findIndexInSelected(recipeCard) > -1;
  }

  findIndexInSelected(recipeCard: RecipeCard) {
    return this.selectedRecipeCards.findIndex(x => x.id === recipeCard.id);
  }
}
