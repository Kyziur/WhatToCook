import { Component, OnInit, effect, model } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, merge, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RecipeCard } from 'src/app/recipes/recipe-card/recipe-card.component';
import { ConsoleLoggerService, LoggerService } from '../logger.service';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroMagnifyingGlass } from '@ng-icons/heroicons/outline';
import { containsIgnoreCase, distinct } from '../utils/utils';
import {
  SelectComponent,
  SelectOption,
} from '../components/select/select.component';
import { filter } from '../components/select/filter';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  standalone: true,
  providers: [
    provideIcons({ heroMagnifyingGlass }),
    { provide: LoggerService, useClass: ConsoleLoggerService },
  ],
  imports: [ReactiveFormsModule, NgIconComponent, SelectComponent],
})
export class SearchComponent implements OnInit {
  isFilteringSectionVisible = false;
  originalItems: RecipeCard[] = [];

  items = model.required<RecipeCard[]>();

  constructor(private logger: LoggerService) {
    effect(() => {
      if (this.originalItems.length === 0) {
        this.originalItems = [...this.items()];
      }
    });
    effect(() => {
      console.error('CHANGE', {
        copy: this.originalItems,
        items: this.items(),
        form: this.form,
      });
    });

    merge(
      this.form.controls.search.valueChanges,
      this.advancedFiltersForm.valueChanges
    )
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(),
        debounceTime(200),
        tap(() => {
          const searchValue = this.form.controls.search.value;
          const tags = this.advancedFiltersForm.value.tags ?? [];
          const ingredients = this.advancedFiltersForm.value.ingredients ?? [];
          const filteredRecipes = this.filter(
            [...this.originalItems],
            searchValue,
            tags.map((x) => x.text),
            ingredients.map((x) => x.text)
          );
          this.items.set(filteredRecipes);

          this.logger.info('Search changed', {
            search: { searchValue, tags, ingredients },
            filteredItems: this.items(),
            allitems: this.originalItems,
          });
        })
      )
      .subscribe();
  }

  ngOnInit(): void {
    console.error('WTF');
  }

  form: FormGroup<{ search: FormControl<string> }> = new FormGroup({
    search: new FormControl<string>('', { nonNullable: true }),
  });

  advancedFiltersForm: FormGroup<{
    tags: FormControl<SelectOption[]>;
    ingredients: FormControl<SelectOption[]>;
  }> = new FormGroup({
    tags: new FormControl<SelectOption[]>([], { nonNullable: true }),
    ingredients: new FormControl<SelectOption[]>([], { nonNullable: true }),
  });

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

  showFilteringSection() {
    this.isFilteringSectionVisible = true;
  }

  getAvailableTags() {
    return distinct(this.originalItems.flatMap((x) => x.tags)).map(
      (el) =>
        ({
          selected: false,
          text: el,
          item: el,
        }) as SelectOption
    );
  }

  getAvailableIngredients() {
    return distinct(this.originalItems.flatMap((x) => x.ingredients)).map(
      (el) =>
        ({
          selected: false,
          text: el,
          item: el,
        }) as SelectOption
    );
  }
}
