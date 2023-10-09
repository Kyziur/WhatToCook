using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
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
    public async Task Given_ValidData_When_CreatingMealPlan_Then_Succeeds()
    {
        //Arrange
        var recipes = new List<Recipe>()
        {
            new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
            new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
        };
        var loggerMock = new Mock<ILogger<MealPlanningService>>();
        _recipesRepositoryMock.Setup(x => x.GetByNames(new[] { "Recipe1", "Recipe2" })).Returns(recipes);
        _mealPlanningRepositoryMock.Setup(x => x.Create(It.IsAny<PlanOfMeals>())).Verifiable(Times.Once);
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

        var planOfMealRequest = new PlanOfMealRequest()
        {
            Id = 0,
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow,
            Recipes = recipes,
            Name = "Test"
        };
        var result = await sut.Create(planOfMealRequest);
        _mealPlanningRepositoryMock.Verify();
    }

    [Fact]
    public async Task Given_ValidData_When_UpdatingMealPlan_Then_ShouldUpdateMealPlan()
    {
        //Arrange old data
        var existingPlanOfMeals = new PlanOfMeals
        (
            "OldTest",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            new List<Recipe>()
            {
                new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
                new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(),  new Statistics(), "path/to/image2", new List<PlanOfMeals>())
            }
        );

        //setup GetMealPlanByName method to return existing plan of meals
        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
             new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(),  new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
             new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(),  new Statistics(), "path/to/image2", new List<PlanOfMeals>())
        };
        //setup GetByNames method to return new recipes
        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);
        var loggerMock = new Mock<ILogger<MealPlanningService>>();
        //arrange instances of interfaces
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

        //arrange new plan of meal data
        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow.AddDays(2),
            ToDate = DateTime.UtcNow.AddDays(6),
            Recipes = recipes
        };
        _mealPlanningRepositoryMock.Setup(x => x.Update(It.IsAny<PlanOfMeals>())).Verifiable(Times.Once);

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
    public async Task Given_GreaterFromDateThanToDate_When_UpdatingMealPlan_Then_ShouldThrowException()
    {
        // Arrange

        var existingPlanOfMeals = new PlanOfMeals
      (
          "OldTest",
          DateTime.UtcNow.AddDays(2),
          DateTime.UtcNow.AddDays(3),
          new List<Recipe>()
          {
              new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
              new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
          }
      );

        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
            new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
            new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
        };

        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);
        var loggerMock = new Mock<ILogger<MealPlanningService>>();

        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow.AddDays(9),
            ToDate = DateTime.UtcNow.AddDays(7),
            Recipes = recipes
        };

        // Act & Assert
        Func<Task> act = () => sut.Update(updatePlanOfMealRequest);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Given_NonExistentRecipes_When_UpdatingMealPlan_Then_ShouldThrowException()
    {
        // Arrange

        var existingPlanOfMeals = new PlanOfMeals
      (
          "OldTest",
          DateTime.UtcNow.AddDays(1),
          DateTime.UtcNow.AddDays(3),
          new List<Recipe>()
          {
              new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
              new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
          }
      );

        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var existingRecipes = new List<Recipe>()

        {
            new Recipe("Recipe1", "Description", "short", new List<Ingredient>(), new Statistics(), "path/to/image", new List<PlanOfMeals>())
        };

        var nonExistentRecipes = new List<Recipe>()

        {
            new Recipe("NonExistentRecipe1", "Description", "short", new List<Ingredient>(), new Statistics(), "path/to/image", new List<PlanOfMeals>())

        };

        var allRecipes = existingRecipes.Concat(nonExistentRecipes).ToList();

        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(existingRecipes);
        var loggerMock = new Mock<ILogger<MealPlanningService>>();
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(6),
            Recipes = allRecipes
        };

        Func<Task> act = () => sut.Update(updatePlanOfMealRequest);
        await act.Should().ThrowAsync<Exception>()
                          .WithMessage("Some recipes do not exist in the database");
        _mealPlanningRepositoryMock.Verify();
    }

    [Fact]
    public async Task Given_PastFromDate_When_UpdatingMealPlan_Then_ThrowsException()
    {
        //Arrange old data
        var existingPlanOfMeals = new PlanOfMeals
        (
            "OldTest",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            new List<Recipe>()
            {
                new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
                new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
            }
        );

        //setup GetMealPlanByName method to return existing plan of meals
        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
             new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
             new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
        };

        //setup GetByNames method to return new recipes
        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);
        var loggerMock = new Mock<ILogger<MealPlanningService>>();

        //arrange instances of interfaces
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);
        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow.AddDays(-1),
            ToDate = DateTime.UtcNow.AddDays(2),
            Recipes = recipes
        };

        // Act and Assert
        await Assert.ThrowsAsync<IncorrectDateException>(() => sut.Update(updatePlanOfMealRequest));
    }

    [Fact]
    public async Task Given_ToDateBeforeFromDate_When_UpdatingMealPlan_Then_ThrowsException()
    {
        //Arrange old data
        var existingPlanOfMeals = new PlanOfMeals
        (
            "OldTest",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            new List<Recipe>()
            {
                new Recipe("OldRecipe1", "Description1", "short", new List<Ingredient>(), new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
                new Recipe("OldRecipe2", "Description2", "medium", new List<Ingredient>(), new Statistics(), "path/to/image2", new List<PlanOfMeals>())
            }
        );

        //setup GetMealPlanByName method to return existing plan of meals
        _mealPlanningRepositoryMock.Setup(x => x.GetMealPlanByName(It.IsAny<string>())).ReturnsAsync(existingPlanOfMeals);

        var recipes = new List<Recipe>()
        {
             new Recipe("Recipe1", "Description1", "short", new List<Ingredient>(),  new Statistics(), "path/to/image1", new List<PlanOfMeals>()),
             new Recipe("Recipe2", "Description2", "medium", new List<Ingredient>(),  new Statistics(), "path/to/image2", new List<PlanOfMeals>())
        };

        //setup GetByNames method to return new recipes
        _recipesRepositoryMock.Setup(x => x.GetByNames(It.IsAny<IEnumerable<string>>())).Returns(recipes);
        var loggerMock = new Mock<ILogger<MealPlanningService>>();

        //arrange instances of interfaces
        var sut = new MealPlanningService(_recipesRepositoryMock.Object, _mealPlanningRepositoryMock.Object, loggerMock.Object);

        // Arrange data where ToDate is before FromDate
        var updatePlanOfMealRequest = new UpdatePlanOfMealRequest()
        {
            Name = "NewTest",
            FromDate = DateTime.UtcNow.AddDays(2),
            ToDate = DateTime.UtcNow.AddDays(1),
            Recipes = recipes
        };

        // Act and Assert
        await Assert.ThrowsAsync<IncorrectDateException>(() => sut.Update(updatePlanOfMealRequest));
    }
}
