import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
  model,
  output,
  signal,
  untracked,
} from '@angular/core';
import { CheckboxComponent } from '../checkbox/checkbox.component';

export interface Selectable<T> {
  value: T;
  selected: boolean;
}

type OnlyStringProperties<T> = {
  [K in keyof T]: T[K] extends string ? K : never;
}[keyof T];

@Component({
  selector: 'app-select-list',
  standalone: true,
  template: `<div>
    @for (item of internalItems(); track idx; let idx = $index) {
      <app-checkbox
        (valueChange)="onSelectionChange($event)"
        [(value)]="item.selected"
        [text]="item.value[textKey()]"></app-checkbox>
    }
  </div> `,
  imports: [CheckboxComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SelectListComponent<T extends Pick<T, OnlyStringProperties<T>>> {
  items = input.required<T[]>();
  internalItems = signal<Selectable<T>[]>([]);
  itemsSelected = output<T[]>();

  textKey = input.required<OnlyStringProperties<T>>();

  constructor() {
    effect(() => {
      console.info('Creating copy of items for internal purposes');
      const items = this.items();
      untracked(() => {
        const mappedItems = items.map(el => ({
          selected: false,
          value: el,
        }));
        this.internalItems.set(mappedItems);
      });
    });

    effect(
      () => {
        const internalItems = this.internalItems();
        this.itemsSelected.emit(
          internalItems.filter(x => x.selected).map(x => x.value)
        );
      },
      {
        allowSignalWrites: true,
      }
    );
  }

  onSelectionChange(x: unknown) {}
}
