namespace WhatToCook.Application.DataTransferObjects.Responses
{
    public class ShoppingListResponse
    {
        public IEnumerable<DayWiseIngredientsResponse> IngredientsPerDay { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }
    public class DayWiseIngredientsResponse
    {
        public DateTime Day { get; set; }
        public IEnumerable<string> Ingredients { get; set; }
    }
}
