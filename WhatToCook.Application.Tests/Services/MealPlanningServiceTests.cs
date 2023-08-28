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
}