import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  computed,
  forwardRef,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface ChangeEvent<T> {
  value: T | undefined;
}

export type CheckboxChangeEvent = ChangeEvent<unknown> & { checked: boolean };
@Component({
  selector: 'app-checkbox',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label class="label cursor-pointer">
      <input
        #inputCheckbox
        (change)="handleChange(inputCheckbox)"
        [attr.name]="name"
        [disabled]="disabled"
        [checked]="isChecked()"
        type="checkbox"
        class="checkbox" />
      <span class="label-text">{{ text() }}</span>
    </label>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CheckboxComponent),
      multi: true,
    },
  ],
})
export class CheckboxComponent implements ControlValueAccessor {
  value = model.required<unknown>();
  isBinary = input<boolean>(true);
  text = input.required<string>();
  name = `Checkbox_${self.crypto.randomUUID()}`;

  isChecked = computed(() => {
    return Boolean(this.value());
  });

  disabled = false;
  touched = false;
  onTouched = () => {};
  onChange = (value: unknown | undefined) => {};

  writeValue(obj: unknown): void {
    this.value.set(obj);
  }

  registerOnChange(fn: (val: unknown | undefined) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleChange(input: HTMLInputElement) {
    if (!this.onChange) {
      return;
    }

    let checkboxValue: unknown | undefined | boolean;
    if (this.isBinary()) {
      checkboxValue = input.checked;
    } else {
      checkboxValue = input.checked ? this.value() : undefined;
    }

    this.value.set(checkboxValue);
    this.onChange(this.value());
    this.onTouched();
  }
}
