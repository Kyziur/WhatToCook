namespace WhatToCook.Application.DataTransferObjects.Responses
{
    public class ShoppingListResponse
    {
        public IEnumerable<string> Ingredients { get; set;}
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
