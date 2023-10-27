using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Exceptions;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.Application.Services;

namespace WhatToCook.Application.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<ILogger<RecipeService>> _loggerMock;
    private readonly Mock<IRecipesRepository> _recipesRepositoryMock;

    public RecipeServiceTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
        _loggerMock = new Mock<ILogger<RecipeService>>();
    }

    [Fact]
    public async Task Given_IncorrectData_When_UpdatingRecipe_ThenThrowsException()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _loggerMock.Object);

        var invalidBase64Image = "INVALID_BASE64";
        var recipeName = "Test123";

        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), new List<RecipePerDay>());
        var mockExistingRecipe = new Recipe(recipeName, "desc", "long", new List<Ingredient>(), new Statistics(), "imagePath", mockRecipePlanOfMealsList);

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        var recipeUpdateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = recipeName,
            Ingredients = new List<string> { "chicken", "rice", "salt" },
            Image = invalidBase64Image,
            PreparationDescription = "",
            TimeToPrepare = "medium"
        };

        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(recipeName)).ReturnsAsync(mockExistingRecipe);

        await Assert.ThrowsAsync<FormatException>(() => sut.Update(recipeUpdateRequest, ""));
    }

    [Fact]
    public async Task Given_InvalidData_When_DeletingRecipe_ThenThrowsException()
    {
        var nonExistingRecipeId = 999;

        _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).ThrowsAsync(new Exception("Recipe not found"));
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);

        await Assert.ThrowsAsync<Exception>(() => sut.Delete(nonExistingRecipeId));

        _recipesRepositoryMock.Verify(x => x.Delete(nonExistingRecipeId), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidRecipeData_When_Updating_ThenThrowsInvalidDataException()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _loggerMock.Object);

        var invalidBase64Image = "INVALID_IMAGE_STRING";
        var recipeName = "ExistingRecipe";

        var updateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = "",
            Ingredients = new List<string>(),
            Image = invalidBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        var imagesDirectory = "some directory";

        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), new List<RecipePerDay>());
        var mockExistingRecipe = new Recipe(recipeName, "desc", "long", new List<Ingredient>(), new Statistics(), "imagePath", mockRecipePlanOfMealsList);

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(recipeName)).ReturnsAsync(mockExistingRecipe);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Update(updateRequest, imagesDirectory));

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task Given_NonExistentRecipe_When_Updating_ThenThrowsNotFoundException()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _loggerMock.Object);

        var validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        var nonexistentRecipeName = "NonexistentRecipe";

        var updateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = nonexistentRecipeName,
            Ingredients = new List<string> { "chicken", "rice", "salt" },
            Image = validBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        var imagesDirectory = "some directory";

        _recipesRepositoryMock.Setup<Task<Recipe?>>(x => x.GetRecipeByName(nonexistentRecipeName)).ReturnsAsync((Recipe?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => sut.Update(updateRequest, imagesDirectory));

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task Given_ValidData_When_CreatingRecipe_ThenSucceeds()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _loggerMock.Object);

        var validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";

        var recipeRequest = new RecipeRequest()
        {
            Name = "Test123",
            Ingredients = new List<string> { "boczus", "jajeczka", "maslo" },
            Image = validBase64Image,
            PreparationDescription = "124asfafafas",
            TimeToPrepare = "short"
        };

        var imagesDirectory = "some directory";
        var expectedImagePath = "some path";

        _recipesRepositoryMock.Setup(x => x.Create(It.IsAny<Recipe>())).Returns(Task.CompletedTask).Verifiable();
        _recipesRepositoryMock.Setup(x => x.SaveImage(It.IsAny<ImageInfo>())).ReturnsAsync(expectedImagePath).Verifiable();
        var result = await sut.Create(recipeRequest, imagesDirectory);

        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be("Test123");
        result.Ingredients.Should().HaveCount(3);
        Assert.Equal(expectedImagePath, result.Image);
        Assert.Equal(recipeRequest.Name, result.Name);
        Assert.Equal(recipeRequest.PreparationDescription, result.Description);
        Assert.Equal(recipeRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(recipeRequest.Ingredients.Count, result.Ingredients.Count);

        _recipesRepositoryMock.Verify(x => x.Create(It.IsAny<Recipe>()), Times.Once);
    }

    [Fact]
    public async Task Given_ValidData_When_DeletingRecipe_Then_RecipeIsDeleted()
    {
        var existingRecipe = new Recipe
            (
            "oldname1",
            "olddescription",
            "short",
            new List<Ingredient>()
            {
                    new Ingredient("old ingredient1"),
                    new Ingredient("old ingredient2"),
                    new Ingredient("old ingredient3"),
                    new Ingredient("old ingredient4"),
            },
            new Statistics(),
            "oldimage.png",
            new List<RecipePlanOfMeals>()
            );

        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), new List<RecipePerDay>());
        var mockRecipePlanOfMeals = new RecipePlanOfMeals(existingRecipe, mockPlanOfMeals, DateTime.UtcNow);
        existingRecipe.RecipePlanOfMeals.Add(mockRecipePlanOfMeals);

        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);

        _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Returns(Task.CompletedTask);

        await sut.Delete(existingRecipe.Id);

        _recipesRepositoryMock.Verify(x => x.Delete(existingRecipe.Id), Times.Once);
    }

    [Fact]
    public async Task Given_ValidUpdateData_When_UpdatingRecipe_ThenSucceeds()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _loggerMock.Object);

        var validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        var oldRecipeName = "Test123";
        var newRecipeName = "UpdatedTest123";

        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), new List<RecipePerDay>());
        var mockExistingRecipe = new Recipe(oldRecipeName, "desc", "long", new List<Ingredient>(), new Statistics(), "imagePath", mockRecipePlanOfMealsList);

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        var recipeUpdateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = newRecipeName,
            Ingredients = new List<string> { "chicken", "rice", "salt" },
            Image = validBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        var imagesDirectory = "some updated directory";

        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(newRecipeName)).ReturnsAsync(mockExistingRecipe);
        _recipesRepositoryMock.Setup(x => x.SaveImage(It.IsAny<ImageInfo>())).ReturnsAsync("some updated path").Verifiable();
        _recipesRepositoryMock.Setup(x => x.Update(It.IsAny<Recipe>())).Returns(Task.CompletedTask).Verifiable();

        var result = await sut.Update(recipeUpdateRequest, imagesDirectory);

        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be(newRecipeName);
        result.Ingredients.Should().HaveCount(3);
        Assert.Equal("some updated path", result.Image);
        Assert.Equal(recipeUpdateRequest.Name, result.Name);
        Assert.Equal(recipeUpdateRequest.PreparationDescription, result.Description);
        Assert.Equal(recipeUpdateRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(recipeUpdateRequest.Ingredients.Count, result.Ingredients.Count);

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Once);
    }
}