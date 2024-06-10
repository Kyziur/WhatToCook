import { TimeToPrepare } from './prepare-time-to-badge.pipe';

export interface Recipe {
  id: number;
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: TimeToPrepare;
  imagePath: string;
  tags: string[];
}

export const EMPTY_RECIPE: Recipe = {
  id: 0,
  name: '',
  ingredients: [],
  preparationDescription: '',
  timeToPrepare: 'Short',
  imagePath: '',
  tags: [],
};
