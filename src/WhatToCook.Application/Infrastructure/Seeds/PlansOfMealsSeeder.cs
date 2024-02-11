using Microsoft.Extensions.DependencyInjection;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Seeds;

internal static class PlansOfMealsSeeder
{
	internal static IServiceProvider SeedMealPlans(this IServiceProvider serviceProvider)
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		DatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		if (dbContext.PlanOfMeals.Any())
		{
			return serviceProvider;
		}

		var recipes = dbContext.Recipes.ToList();
		IEnumerable<PlanOfMeals> data = CreateMealPlans(recipes);
		dbContext.PlanOfMeals.AddRange(data);
		dbContext.SaveChanges();

		return serviceProvider;
	}

	private static IEnumerable<PlanOfMeals> CreateMealPlans(List<Recipe> recipes)
	{
		var random = new Random();
		IEnumerable<PlanOfMeals> mealPlans = Enumerable
			.Range(0, 50)
			.Select(x =>
		new PlanOfMeals
		(
			  $"Meal plan {x}",
			  DateTime.UtcNow.AddDays(x),
			  DateTime.UtcNow.AddDays(x + 7),
			  Enumerable
			  .Range(0, random.Next(2, 9))
			  .Select((rpd, count) => new RecipePerDay(
				  DateTime.UtcNow.AddDays(count),
				  random.GetItems<Recipe>(recipes.ToArray(), random.Next(1, 5)).First()
				)).ToList()));

		return mealPlans;
	}
}