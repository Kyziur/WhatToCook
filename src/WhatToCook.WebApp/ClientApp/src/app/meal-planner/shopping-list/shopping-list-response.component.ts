export interface shoppingListResponse {
  fromDate: Date;
  toDate: Date;
  ingredientsPerDay: dayWiseIngredientsResponse[];
}
export interface dayWiseIngredientsResponse {
  day: Date;
  ingredients: string[];
}
