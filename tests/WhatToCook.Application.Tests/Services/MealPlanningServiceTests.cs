using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.Application.Services;

namespace WhatToCook.Application.Tests.Services;

public class MealPlanningServiceTests
{
	private readonly Mock<IMealPlanningRepository> _mealPlanningRepositoryMock;
	private readonly Mock<IRecipesRepository> _recipesRepositoryMock;

	public MealPlanningServiceTests()
	{
		_recipesRepositoryMock = new Mock<IRecipesRepository>();
		_mealPlanningRepositoryMock = new Mock<IMealPlanningRepository>();
	}

	[Fact]
	public async Task Given_InvalidDateRange_When_UpdatingMealPlan_Then_ShouldThrowException()
	{
		DateTime oldStartDate = DateTime.UtcNow.AddDays(1);
		DateTime oldEndDate = DateTime.UtcNow.AddDays(4);

		DateTime recipeDate1 = DateTime.UtcNow.AddDays(2);
		DateTime recipeDate2 = DateTime.UtcNow.AddDays(3);

		var oldRecipesPerDay = new List<RecipePerDay>

		{
		new RecipePerDay(recipeDate1, new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1")),
		new RecipePerDay(recipeDate2, new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2"))
		};

		var existingPlanOfMeals = new PlanOfMeals("OldTest", oldStartDate, oldEndDate, oldRecipesPerDay);

		_mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

		DateTime newStartDate = DateTime.UtcNow.AddDays(8);
		DateTime newEndDate = DateTime.UtcNow.AddDays(6);
		var daysInRange = Enumerable.Range(0, (newEndDate - newStartDate).Days + 1).Select(x => newStartDate.AddDays(x)).ToList();

		var newRecipes = new List<Recipe>

		{
		new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1"),
		new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2")
		};

		var planOfMealsForDayList = daysInRange.Select(day => new PlanOfMealForDay
		{
			Day = day,
			RecipesIds = newRecipes.Select(r => r.Id).ToList(),
		}).ToList();

		_recipesRepositoryMock.Setup(x => x.GetRecipesByIdForMealPlan(It.IsAny<IEnumerable<int>>()))
			.Returns<IEnumerable<int>>(ids => newRecipes.Where(r => ids.Contains(r.Id)).ToList());

		var loggerMock = new Mock<ILogger<MealPlanningService>>();
		PlanOfMeals? capturedPlan = null;
		_mealPlanningRepositoryMock.Setup(x => x.Update(It.IsAny<PlanOfMeals>())).Callback<PlanOfMeals>(p => capturedPlan = p);

		var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

		var updatePlanOfMealRequest = new UpdatePlanOfMealRequest
		{
			Id = existingPlanOfMeals.Id,
			Name = "InvalidTest",
			FromDate = newStartDate,
			ToDate = newEndDate,
			Recipes = planOfMealsForDayList
		};

		var exception = await Assert.ThrowsAsync<IncorrectDateException>(async () => await sut.Update(updatePlanOfMealRequest));
		Assert.Equal("ToDate cannot be lower than FromDate.", exception.Message);
		_recipesRepositoryMock.Verify();
		_mealPlanningRepositoryMock.Verify();
	}

	[Fact]
	public async Task Given_NonExistentRecipes_When_UpdatingMealPlan_Then_ShouldThrowException()
	{
		DateTime oldStartDate = DateTime.UtcNow.AddDays(1);
		DateTime oldEndDate = DateTime.UtcNow.AddDays(4);

		DateTime recipeDate1 = DateTime.UtcNow.AddDays(2);
		DateTime recipeDate2 = DateTime.UtcNow.AddDays(3);

		var oldRecipesPerDay = new List<RecipePerDay>

		{
		new RecipePerDay(recipeDate1, new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1")),
		new RecipePerDay(recipeDate2, new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2"))
		};

		var existingPlanOfMeals = new PlanOfMeals("OldTest", oldStartDate, oldEndDate, oldRecipesPerDay);

		_mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

		DateTime newStartDate = DateTime.UtcNow.AddDays(4);
		DateTime newEndDate = DateTime.UtcNow.AddDays(6); ;
		var daysInRange = Enumerable.Range(0, (newEndDate - newStartDate).Days + 1).Select(x => newStartDate.AddDays(x)).ToList();

		var newRecipes = new List<Recipe>

		{
		new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1"),
		new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2")
		};

		var nonExistentRecipeId = 9999;
		var recipeId = 0;
		var planOfMealsForDayList = new List<PlanOfMealForDay>
		{
		new PlanOfMealForDay

		{
			Day = newStartDate,
			RecipesIds = new List<int> { nonExistentRecipeId }
		},
		new PlanOfMealForDay
		{
			Day = newStartDate,
			RecipesIds = new List<int> { recipeId }
		}
	};

		_recipesRepositoryMock.Setup(x => x.GetRecipesByIdForMealPlan(It.IsAny<IEnumerable<int>>()))
			.Returns<IEnumerable<int>>(ids => newRecipes.Where(r => ids.Contains(r.Id)).ToList());

		var loggerMock = new Mock<ILogger<MealPlanningService>>();
		PlanOfMeals? capturedPlan = null;
		_mealPlanningRepositoryMock.Setup(x => x.Update(It.IsAny<PlanOfMeals>())).Callback<PlanOfMeals>(p => capturedPlan = p);

		var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

		var updatePlanOfMealRequest = new UpdatePlanOfMealRequest
		{
			Id = existingPlanOfMeals.Id,
			Name = "TestWithNonExistentRecipe",
			FromDate = newStartDate,
			ToDate = newEndDate,
			Recipes = planOfMealsForDayList
		};

		Func<Task> act = () => sut.Update(updatePlanOfMealRequest);
		await act.Should().ThrowAsync<Exception>();

		_recipesRepositoryMock.Verify();
		_mealPlanningRepositoryMock.Verify();
	}

	[Fact]
	public async Task Given_ValidData_When_CreatingMealPlan_Then_Succeeds()
	{
		DateTime startDate = DateTime.UtcNow;
		DateTime endDate = DateTime.UtcNow.AddDays(7);
		var daysInRange = Enumerable.Range(0, (endDate - startDate).Days + 1).Select(x => startDate.AddDays(x)).ToList();

		var recipes = new List<Recipe>()
		{
			new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1"),
			new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2"),
			new Recipe("Recipe3", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2")
		};
		var planOfMealsForDayList = daysInRange.Select(day => new PlanOfMealForDay
		{
			Day = day,
			RecipesIds = recipes.Select(r => r.Id).ToList(),
		}).ToList();

		var loggerMock = new Mock<ILogger<MealPlanningService>>();
		_recipesRepositoryMock.Setup(x => x.GetRecipesByIdForMealPlan(It.IsAny<IEnumerable<int>>())).Returns(recipes);
		PlanOfMeals? capturedPlan = null;

		_mealPlanningRepositoryMock.Setup(x => x.Create(It.IsAny<PlanOfMeals>())).Callback<PlanOfMeals>(p => capturedPlan = p);
		var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

		var planOfMealRequest = new PlanOfMealRequest()
		{
			Id = 0,
			FromDate = startDate,
			ToDate = endDate,
			Recipes = planOfMealsForDayList,
			Name = "Test"
		};
		var result = await sut.Create(planOfMealRequest);
		Assert.NotNull(capturedPlan);
		Assert.Equal(planOfMealRequest.Name, capturedPlan.Name);
		Assert.Equal(planOfMealRequest.FromDate, capturedPlan.FromDate);
		Assert.Equal(planOfMealRequest.ToDate, capturedPlan.ToDate);
		Assert.All(planOfMealsForDayList, p => Assert.NotNull(p.RecipesIds));

		foreach (var dayPlan in planOfMealsForDayList)
		{
			var capturedDayPlans = capturedPlan.RecipePlanOfMeals
				.Where(cp => cp.Day == dayPlan.Day)
				.ToList();

			Assert.NotNull(capturedDayPlans);

			var actualRecipeIds = capturedDayPlans.Select(cp => cp.RecipeId).ToList();

			var actualRecipeNames = recipes
				.Where(r => actualRecipeIds.Contains(r.Id))
				.Select(r => r.Name)
				.OrderBy(name => name)
				.ToList();

			var expectedRecipeNames = recipes
				.Where(r => dayPlan.RecipesIds.Contains(r.Id))
				.Select(r => r.Name)
				.OrderBy(name => name)
				.ToList();

			Assert.Equal(expectedRecipeNames, actualRecipeNames);
		}

		foreach (var dayPlan in planOfMealsForDayList)
		{
			var duplicateRecipes = recipes
				.Where(r => dayPlan.RecipesIds.Contains(r.Id))
				.GroupBy(r => r.Name)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();
			var errorMessage = $"Found duplicate recipe: {string.Join(", ", duplicateRecipes)} on day {dayPlan.Day}";
			Assert.True(duplicateRecipes.Count == 0, errorMessage);

			var capturedDayPlan = capturedPlan.RecipePlanOfMeals
			 .Where(x => x.RecipeId == capturedPlan.Id).ToList();
		}
		_recipesRepositoryMock.Verify();
		_mealPlanningRepositoryMock.Verify();
	}

	[Fact]
	public async Task Given_ValidData_When_UpdatingMealPlan_Then_ShouldUpdateMealPlan()
	{
		DateTime oldStartDate = DateTime.UtcNow.AddDays(1);
		DateTime oldEndDate = DateTime.UtcNow.AddDays(4);

		DateTime recipeDate1 = DateTime.UtcNow.AddDays(2);
		DateTime recipeDate2 = DateTime.UtcNow.AddDays(3);

		var oldRecipesPerDay = new List<RecipePerDay>
		{
		new RecipePerDay(recipeDate1, new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1")),
		new RecipePerDay(recipeDate2, new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2"))
		};

		var existingPlanOfMeals = new PlanOfMeals("OldTest", oldStartDate, oldEndDate, oldRecipesPerDay);

		_mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

		DateTime newStartDate = DateTime.UtcNow.AddDays(2);
		DateTime newEndDate = DateTime.UtcNow.AddDays(6); ;
		var daysInRange = Enumerable.Range(0, (newEndDate - newStartDate).Days + 1).Select(x => newStartDate.AddDays(x)).ToList();

		var newRecipes = new List<Recipe>

		{
		new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1"),
		new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2")
		};

		var planOfMealsForDayList = daysInRange.Select(day => new PlanOfMealForDay
		{
			Day = day,
			RecipesIds = newRecipes.Select(r => r.Id).ToList(),
		}).ToList();

		_recipesRepositoryMock.Setup(x => x.GetRecipesByIdForMealPlan(It.IsAny<IEnumerable<int>>()))
			.Returns<IEnumerable<int>>(ids => newRecipes.Where(r => ids.Contains(r.Id)).ToList());

		var loggerMock = new Mock<ILogger<MealPlanningService>>();
		PlanOfMeals? capturedPlan = null;
		_mealPlanningRepositoryMock.Setup(x => x.Update(It.IsAny<PlanOfMeals>())).Callback<PlanOfMeals>(p => capturedPlan = p);

		var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

		var updatePlanOfMealRequest = new UpdatePlanOfMealRequest
		{
			Id = existingPlanOfMeals.Id,
			Name = "NewTest",
			FromDate = newStartDate,
			ToDate = newEndDate,
			Recipes = planOfMealsForDayList
		};

		var result = await sut.Update(updatePlanOfMealRequest);

		Assert.NotNull(capturedPlan);
		Assert.Equal(updatePlanOfMealRequest.Name, capturedPlan.Name);
		Assert.Equal(updatePlanOfMealRequest.FromDate, capturedPlan.FromDate);
		Assert.Equal(updatePlanOfMealRequest.ToDate, capturedPlan.ToDate);
		Assert.All(planOfMealsForDayList, p => Assert.NotNull(p.RecipesIds));

		foreach (var dayPlan in planOfMealsForDayList)
		{
			var capturedDayPlans = capturedPlan.RecipePlanOfMeals
				.Where(cp => cp.Day == dayPlan.Day)
				.ToList();

			Assert.NotNull(capturedDayPlans);

			var actualRecipeIds = capturedDayPlans.Select(cp => cp.RecipeId).ToList();
			var actualRecipeNames = newRecipes
				.Where(r => actualRecipeIds.Contains(r.Id))
				.Select(r => r.Name)
				.OrderBy(name => name)
				.ToList();

			var expectedRecipeNames = newRecipes
				.Where(r => dayPlan.RecipesIds.Contains(r.Id))
				.Select(r => r.Name)
				.OrderBy(name => name)
				.ToList();

			Assert.Equal(expectedRecipeNames, actualRecipeNames);
		}

		foreach (var dayPlan in planOfMealsForDayList)
		{
			var duplicateRecipes = newRecipes
				.Where(r => dayPlan.RecipesIds.Contains(r.Id))
				.GroupBy(r => r.Name)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			var errorMessage = $"Found duplicate recipe: {string.Join(", ", duplicateRecipes)} on day {dayPlan.Day}";
			Assert.True(duplicateRecipes.Count == 0, errorMessage);

			var capturedDayPlan = capturedPlan.RecipePlanOfMeals
			 .Where(x => x.RecipeId == capturedPlan.Id).ToList();
		}

		_recipesRepositoryMock.Verify();
		_mealPlanningRepositoryMock.Verify();
	}
}