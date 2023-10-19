export interface shoppingListResponse {
    ingredients: string[];
    fromDate: Date;
    toDate: Date;
    dayWiseIngredientsList?: DayWiseIngredients[];
}
export interface DayWiseIngredients {
    date: Date;
    ingredients: string[];
}