export type CreatePlanOfMealApi = Omit<PlanOfMealApiBase, 'id'>;

export type UpdatePlanOfMealApi = PlanOfMealApiBase;

export type GetPlanOfMealApi = PlanOfMealApiBase;

export interface GetPlansOfMealsApi {
  mealPlans: GetPlanOfMealApi[];
}

export interface PlanOfMealForDayApi {
  day: Date;
  recipeIds: number[];
}

interface PlanOfMealApiBase {
  id: number;
  name: string;
  recipes: PlanOfMealForDayApi[];
  fromDate: Date;
  toDate: Date;
}
