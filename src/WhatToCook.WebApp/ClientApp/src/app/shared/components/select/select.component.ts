import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  computed,
  effect,
  forwardRef,
  input,
  model,
  signal,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';
import { CheckboxComponent } from '../checkbox/checkbox.component';

export interface SelectOption {
  selected: boolean;
  text: string;
  item: unknown;
}

@Component({
  selector: 'app-select',
  standalone: true,
  templateUrl: './select.component.html',
  imports: [CheckboxComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true,
    },
  ],
})
export class SelectComponent implements ControlValueAccessor {
  toggleIsOpen() {
    this.isOpen.update((isOpen) => !isOpen);
  }

  label = input.required<string>();
  isOpen = model<boolean>(false);
  options = input.required<SelectOption[]>();
  filterByKey = input<string>('');
  filter = signal<string>('');
  value: SelectOption[] = [];

  touched: boolean = false;
  isDisabled: boolean = false;
  onChange = (value: SelectOption[]) => {};
  onTouched = () => {};

  filteredOptions = computed(() => {
    return this.filter().length === 0
      ? this.options()
      : this.options().filter((x) =>
          x.text.toLowerCase().includes(this.filter().toLowerCase())
        );
  });

  writeValue(obj: SelectOption[]): void {
    this.value = obj;
  }

  registerOnChange(fn: (value: SelectOption[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  isSelected(item: SelectOption) {
    return this.value.some((x) => x.text === item.text);
  }

  onFilterChange(event: Event) {
    event.preventDefault();
    const value = (event.target as HTMLInputElement).value as string;
    this.filter.set(value);
  }

  handleOptionSelectionChange($event: boolean, index: number) {
    const option = this.filteredOptions().at(index);
    if (!option) {
      return;
    }

    option.selected = $event;

    if ($event && !this.isValueAlreadyPresent(option)) {
      this.value = [...this.value, option];
    } else {
      this.value = this.value.filter((x) => x.text !== option.text);
    }

    this.onChange(this.value);
  }

  isValueAlreadyPresent(option: SelectOption) {
    if (this.value.length === 0) {
      return false;
    }

    return this.value.some((x) => x.text === option.text);
  }
}
