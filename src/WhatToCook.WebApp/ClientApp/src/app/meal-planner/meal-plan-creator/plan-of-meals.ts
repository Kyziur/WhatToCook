import { Recipe } from "src/app/recipes/Recipe";

export interface PlanOfMeals {
    
    id: number;
    name: string;
    fromDate: Date;
    toDate: Date;
    recipes: Recipe[];
}