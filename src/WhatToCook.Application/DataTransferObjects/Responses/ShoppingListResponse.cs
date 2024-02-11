namespace WhatToCook.Application.DataTransferObjects.Responses;

public class ShoppingListResponse
{
    public IEnumerable<DayWiseIngredientsResponse> IngredientsPerDay { get; set; } = Enumerable.Empty<DayWiseIngredientsResponse>();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class DayWiseIngredientsResponse
{
    public DateTime Day { get; set; }

    public IEnumerable<string> Ingredients { get; private set; }

    public DayWiseIngredientsResponse(DateTime day, IEnumerable<string> ingredients)
    {
        Day = day;
        Ingredients = ingredients;
    }
}