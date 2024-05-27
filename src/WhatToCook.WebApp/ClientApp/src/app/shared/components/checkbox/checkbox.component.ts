import {
  Component,
  ViewChild,
  computed,
  forwardRef,
  input,
  model,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-checkbox',
  standalone: true,
  imports: [],
  template: `
    <label class="label cursor-pointer">
      <input
        #inputCheckbox
        [attr.name]="name()"
        [value]="value()"
        [disabled]="disabled"
        checked="{{ isChecked() }}"
        (change)="handleChange(inputCheckbox)"
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
  value = model.required<boolean>();
  text = input.required<string>();
  name = input<string>(`Checkbox_${Date.now()}`);

  isChecked = computed(() => {
    return this.value() ? 'checked' : '';
  });

  disabled = false;
  touched = false;
  onTouched = () => {};
  onChange = (value: boolean) => {};

  writeValue(obj: boolean): void {
    this.value.set(obj);
  }

  registerOnChange(fn: (val: boolean) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleChange(input: HTMLInputElement) {
    this.value.update(() => input.checked);
    this.onChange(this.value());
  }
}
