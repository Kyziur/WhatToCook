import { Component, effect, input, model, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RecipeCard } from 'src/app/recipes/recipe-card/recipe-card.component';
import { ConsoleLoggerService, LoggerService } from '../logger.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroMagnifyingGlass } from '@ng-icons/heroicons/outline';
import { distinct } from '../utils/utils';
import { SelectComponent } from '../components/select/select.component';
import { filter } from '../components/select/filter';
import { ModalComponent } from '../modal/modal.component';
import { CheckboxComponent } from '../components/checkbox/checkbox.component';
import { SelectListComponent } from '../components/select-list/select-list.component';
export interface SearchItem {
  item: string;
}
@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  standalone: true,
  providers: [
    provideIcons({ heroMagnifyingGlass }),
    { provide: LoggerService, useClass: ConsoleLoggerService },
  ],
  imports: [
    ReactiveFormsModule,
    NgIconComponent,
    SelectComponent,
    ModalComponent,
    CheckboxComponent,
    SelectListComponent,
    FormsModule,
  ],
})
export class SearchComponent {
  showIngredientsFilter() {
    this.isIngredientsFilterVisible = true;
  }
  show() {
    console.error('this', this.tags);
  }

  isTagFilterVisible = false;
  isIngredientsFilterVisible = false;
  originalItems: RecipeCard[] = [];
  items = model.required<RecipeCard[]>();

  tags = input.required<SearchItem[], string[]>({
    transform: tags => {
      return tags.map(
        tag =>
          ({
            item: tag,
          }) as SearchItem
      );
    },
  });

  ingredients = input.required<SearchItem[], string[]>({
    transform: ingredients => {
      return ingredients.map(
        ingredient =>
          ({
            item: ingredient,
          }) as SearchItem
      );
    },
  });

  searchPhrase = signal<string>('');
  selectedTags = model<SearchItem[]>([]);
  selectedIngredients = model<SearchItem[]>([]);

  constructor(private logger: LoggerService) {
    effect(
      () => {
        const search = this.searchPhrase();
        const tags = this.selectedTags();
        const ingredients = this.selectedIngredients();
        const items = this.items();
        const filteredItems = this.filter(
          this.items(),
          this.searchPhrase(),
          this.selectedTags().map(x => x.item),
          this.selectedIngredients().map(x => x.item)
        );
        this.items.set(filteredItems);
      },
      {
        allowSignalWrites: true,
      }
    );
  }

  filter(
    recipes: RecipeCard[],
    phrase: string,
    tags: string[],
    ingredients: string[]
  ) {
    return filter(recipes, {
      phrase,
      selectedTags: tags,
      selectedIngredients: ingredients,
    });
  }

  showTagsFilter() {
    this.isTagFilterVisible = true;
  }
}
