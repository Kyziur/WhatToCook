import { TimeToPrepare } from '../prepare-time-to-badge.pipe';

export interface CreateRecipe {
  id: number;
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: TimeToPrepare;
  image: string;
  tags: string[];
}
