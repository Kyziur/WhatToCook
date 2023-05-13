
export interface CreateRecipe {
  id: number;
  name: string;
  ingredients: string[];
  preparationDescription: string;
  timeToPrepare: string;
  image: string;
}
