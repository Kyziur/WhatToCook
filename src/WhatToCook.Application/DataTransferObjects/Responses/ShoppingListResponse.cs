namespace WhatToCook.Application.DataTransferObjects.Responses
{
    public class ShoppingListResponse
    {
        public IEnumerable<string> Ingredients { get; set;}
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<DayWiseIngredients> DayWiseIngredientsList { get; set; }
    }
    public class DayWiseIngredients
    {
        public DateTime Date { get; set; }
        public List<string> Ingredients { get; set; }
    }
}
