import { Recipe } from "src/app/recipes/Recipe";

export interface PlanOfMeals {
    
    Id: number;
    Name: string;
    FromDate: Date;
    ToDate: Date;
    Recipes: Recipe[];
}
