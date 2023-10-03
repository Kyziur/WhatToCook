import {TimeToPrepare} from "./TimeToPrepare";

export interface Recipe {
  id: number;
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: TimeToPrepare;
  imagePath: string;
  tags: string[]
}

