import { Pipe, PipeTransform } from '@angular/core';
import { Badge } from '../shared/badge/badge.component';

export type TimeToPrepare = 'Short' | 'Medium' | 'Long';
export const TimeToPrepareValues: TimeToPrepare[] = ['Short', 'Medium', 'Long'];

@Pipe({
  name: 'prepareTimeToBadgePipe',
  standalone: true,
})
export class PrepareTimeToBadgePipe implements PipeTransform {
  transform(value: TimeToPrepare): Badge {
    switch (value) {
      case 'Short':
        return {
          level: 'success',
          text: value,
        };
      case 'Medium':
        return {
          level: 'warning',
          text: value,
        };
      case 'Long':
        return {
          level: 'error',
          text: value,
        };
    }
  }
}
