using WhatToCook.Application.Domain;

namespace WhatToCook.Application.DataTransferObjects.Requests
{
    public class UpdatePlanOfMealRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public IEnumerable<PlanOfMealForDay> Recipes { get; set; } = Enumerable.Empty<PlanOfMealForDay>();
    }
    public class UpdatePlanOfMealForDay
    {
        public DateTime Day { get; set; }
        public IEnumerable<int> RecipeIds { get; set; } = Enumerable.Empty<int>();
    }

}
