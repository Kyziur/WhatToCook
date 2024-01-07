import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  forwardRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import {
  ControlValueAccessor,
  FormControl,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';
import { Subject } from 'rxjs';

type OptionalDate = Date | null;
type OnChangeFn<T> = (value: T) => void;
type OnTouchFn = () => void;

@Component({
  selector: 'app-input-date',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './input-date.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: forwardRef(() => InputDateComponent),
    },
    DatePipe,
  ],
})
export class InputDateComponent
  implements ControlValueAccessor, OnInit, OnDestroy
{
  date: OptionalDate = null;
  disabled = false;
  dateControl = new FormControl<string>('', { nonNullable: true });
  destroy$ = new Subject<void>();
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  private onChange: OnChangeFn<OptionalDate> = (date: OptionalDate) => {};
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  private onTouched: OnTouchFn = () => {};
  @ViewChild('datePicker') datePicker?: ElementRef;

  constructor(private datePipe: DatePipe) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.unsubscribe();
  }

  ngOnInit(): void {
    this.dateControl.valueChanges.subscribe(value =>
      this.onChange(new Date(value))
    );
  }

  setDate(date: OptionalDate) {
    this.date = date;
    this.dateControl.patchValue(
      this.datePipe.transform(this.date, 'yyyy-MM-dd') ?? ''
    );
    this.onChange(this.date);
  }

  registerOnChange(onChange: OnChangeFn<OptionalDate>): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: OnTouchFn): void {
    this.onTouched = onTouched;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  writeValue(date: OptionalDate | string): void {
    if (typeof date === 'string') {
      this.setDate(new Date(date));
      return;
    }

    this.setDate(date);
  }

  showDatePickerOnClick() {
    const input = this.datePicker?.nativeElement as
      | HTMLInputElement
      | undefined;

    input?.showPicker();
  }
}
