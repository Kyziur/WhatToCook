namespace WhatToCook.Application.DataTransferObjects.Requests
{
    public class PlanOfMealForDay
    {
        public DateTime Day { get; set; }
        public IEnumerable<int> RecipeIds { get; set; } = Enumerable.Empty<int>();
    }

    public class PlanOfMealRequest
    {
        public DateTime FromDate { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<PlanOfMealForDay> Recipes { get; set; } = Enumerable.Empty<PlanOfMealForDay>();
        public DateTime ToDate { get; set; }
    }
}