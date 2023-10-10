import { AbstractControl, ValidationErrors } from '@angular/forms';

export function notWhitespaceValidator(
  control: AbstractControl,
): ValidationErrors | null {
  const isWhitespace = (control.value || '').trim().length === 0;
  return isWhitespace ? { whitespace: true } : null;
}
