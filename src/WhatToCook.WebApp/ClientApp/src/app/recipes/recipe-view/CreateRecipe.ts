import { TimeToPrepare } from '../TimeToPrepare';

export interface CreateRecipe {
  id: number;
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: TimeToPrepare;
  image: string;
}
