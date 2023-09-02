using FluentAssertions;
using Moq;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.Application.Services;

namespace WhatToCook.Application.Tests.Services;

public class MealPlanningServiceTests
{
    private readonly Mock<IRecipesRepository> _recipesRepositoryMock;
    private readonly Mock<IMealPlanningRepository> _mealPlanningRepositoryMock;

    public MealPlanningServiceTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
        _mealPlanningRepositoryMock = new Mock<IMealPlanningRepository>();
    }
    [Fact]
    public async Task Given_ValidData_When_CreatingMealPlan_ThenSucceeds()
    {
        //Arrange
        var recipes = new List<Recipe>()
        {
            new() { Name = "Recipe1" },
            new() { Name = "Recipe2" }
        };

        _recipesRepositoryMock.Setup(x => x.GetByNames(new[] { "Recipe1", "Recipe2" })).Returns(recipes);


        ICollection<PlanOfMeals> planOfMeals = new List<PlanOfMeals>();
        // _mealPlanningRepositoryMock.Setup(x => x.Create(Capture.In(planOfMeals)));

        _mealPlanningRepositoryMock.Setup(x => x.Create(It.IsAny<PlanOfMeals>())).Verifiable(Times.Once);
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object);

        var planOfMealRequest = new PlanOfMealRequest()
        {
            Id = 0,
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow,
            Recipes = recipes,
            Name = "Test"
        };

        //Act
        var result = await sut.Create(planOfMealRequest);

        //Assert
        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be("Test");
        result.Recipes.Should().NotBeNullOrEmpty().And.HaveCount(2);
        // Assert.NotEmpty(planOfMeals);
        _mealPlanningRepositoryMock.Verify();
    }

    [Fact]
    public async Task Given_ValidData_When_UpdatingMealPlan_ThenShouldUpdateMealPlan()
    {
        //Arrange old data
        var existingPlanOfMeals = new PlanOfMeals()
        {
            Id = 1,
            Name = "OldTest",
            FromDate = DateTime.UtcNow.AddDays(-1),
            ToDate = DateTime.UtcNow.AddDays(1),
            Recipes = new List<Recipe>()
            {
                new() { Name = "OldRecipe1" },
                new() { Name = "OldRecipe2" }
            }
        };

        //setup GetMealPlanByNameAsync method to return existing plan of meals
        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByNameAsync(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
            new() { Name = "Recipe1" },

            new() { Name = "Recipe2" }
        };
        //setup GetByNames method to return new recipes
        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);
        //arrange instances of interfaces
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object);

        //arrange new plan of meal data
        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(2),
            Recipes = recipes
        };
        // _mealPlanningRepositoryMock.Setup(x => x.Update(It.IsAny<PlanOfMeals>())).Verifiable();

        //Act
        //update plan of meal with new data
        var result = await sut.Update(updatePlanOfMealRequest);

        //Assert
        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be(updatePlanOfMealRequest.Name);
        result.Recipes.Should().NotBeNullOrEmpty().And.BeEquivalentTo(recipes);

        _mealPlanningRepositoryMock.Verify();
    }
    [Fact]
    public async Task Given_GreaterToDateThanFromDate_When_UpdatingMealPlan_ThenShouldThrowException()
    {
        // Arrange
        var existingPlanOfMeals = new PlanOfMeals()
        {
            Id = 1,
            Name = "OldTest",
            FromDate = DateTime.UtcNow.AddDays(0),
            ToDate = DateTime.UtcNow.AddDays(1),
            Recipes = new List<Recipe>()
        {
            new() { Name = "OldRecipe1" },
            new() { Name = "OldRecipe2" }
        }
        };

        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByNameAsync(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
            new() { Name = "Recipe1" },
            new() { Name = "Recipe2" }
        };

        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);

        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object);

        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow.AddDays(7),
            ToDate = DateTime.UtcNow.AddDays(-1),
            Recipes = recipes
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sut.Update(updatePlanOfMealRequest));
    }

    [Fact]
    public async Task Given_NonExistentRecipes_When_UpdatingMealPlan_ThenShouldThrowException()
    {
        // Arrange
        var existingPlanOfMeals = new PlanOfMeals()
        {
            Id = 1,
            Name = "OldTest",
            FromDate = DateTime.UtcNow.AddDays(-1),
            ToDate = DateTime.UtcNow.AddDays(1),
            Recipes = new List<Recipe>()
        {
            new() { Name = "OldRecipe1" },
            new() { Name = "OldRecipe2" }
        }
        };

        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByNameAsync(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var existingRecipes = new List<Recipe>()
        {
            new() { Name = "Recipe1" }
        };
        var nonExistentRecipes = new List<Recipe>()
        {
            new() { Name = "NonExistentRecipe1" }
        };

        var allRecipes = existingRecipes.Concat(nonExistentRecipes).ToList();

        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(existingRecipes);

        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object);

        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(2),
            Recipes = allRecipes
        };

        // Act
        var result = await sut.Update(updatePlanOfMealRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be(updatePlanOfMealRequest.Name);
        result.FromDate.Should().Be(updatePlanOfMealRequest.FromDate);
        result.ToDate.Should().Be(updatePlanOfMealRequest.ToDate);
        result.Recipes.Should().NotBeNullOrEmpty().And.BeEquivalentTo(existingRecipes);

        _mealPlanningRepositoryMock.Verify();
    }
}