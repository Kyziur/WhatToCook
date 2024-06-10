export interface ShoppingListResponse {
  fromDate: Date;
  toDate: Date;
  ingredientsPerDay: DayWiseIngredientsResponse[];
}

export interface DayWiseIngredientsResponse {
  day: Date;
  ingredients: string[];
}
