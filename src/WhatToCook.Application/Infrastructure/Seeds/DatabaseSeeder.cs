namespace WhatToCook.Application.Infrastructure.Seeds;

public static class DatabaseSeeder
{
	public static void SeedDatabase(this IServiceProvider serviceProvider)
	{
		serviceProvider
			.SeedRecipes()
			.SeedMealPlans();
	}
}