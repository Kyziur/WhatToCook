import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RecipeCard } from 'src/app/recipes/recipe-card/recipe-card.component';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  standalone: true,
  imports: [ReactiveFormsModule],
})
export class SearchComponent {
  itemsCopy: Array<RecipeCard> = [];
  #items: Array<RecipeCard> = [];

  @Output() itemsChange = new EventEmitter<RecipeCard[]>();
  @Input({ required: true })
  public set items(value: Array<RecipeCard>) {
    this.#items = value;
    if (this.itemsCopy.length === 0) {
      this.itemsCopy = value;
    }
  }
  public get items(): Array<RecipeCard> {
    return this.#items;
  }

  constructor() {
    this.form.controls.search.valueChanges
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(),
        debounceTime(200),
        tap((value) => {
          this.#items = this.filter(this.itemsCopy, value);
          this.itemsChange.emit(this.items);
          console.error('items', {
            copy: this.itemsCopy,
            searchRes: this.items,
            phrase: value,
          });
        })
      )
      .subscribe();
  }

  form: FormGroup<{ search: FormControl<string> }> = new FormGroup({
    search: new FormControl<string>('', { nonNullable: true }),
  });

  filter(recipes: RecipeCard[], phrase: string) {
    return phrase.length === 0
      ? recipes
      : recipes.filter((recipe) => recipe.name.includes(phrase));
  }
}
