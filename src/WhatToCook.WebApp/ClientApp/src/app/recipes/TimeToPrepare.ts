import { Badge } from '../shared/badge/badge.component';

export type TimeToPrepare = 'Short' | 'Medium' | 'Long';
export const TimeToPrepareValues: TimeToPrepare[] = ['Short', 'Medium', 'Long'];

export function mapTimeToPrepareToBadge(timeToPrepare: TimeToPrepare): Badge {
  switch (timeToPrepare) {
    case 'Short':
      return {
        level: 'success',
        text: timeToPrepare,
      };
    case 'Medium':
      return {
        level: 'warning',
        text: timeToPrepare,
      };
    case 'Long':
      return {
        level: 'error',
        text: timeToPrepare,
      };
  }
}
