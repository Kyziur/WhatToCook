export interface ShoppingListResponse {
  fromDate: Date;
  toDate: Date;
  ingredientsPerDay: dayWiseIngredientsResponse[];
}
export interface dayWiseIngredientsResponse {
  day: Date;
  ingredients: string[];
}
