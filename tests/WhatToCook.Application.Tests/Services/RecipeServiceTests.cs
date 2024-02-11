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
    private readonly Mock<ITagsRepository> _tagsRepositoryMock;

    public RecipeServiceTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
        _loggerMock = new Mock<ILogger<RecipeService>>();
        _tagsRepositoryMock = new Mock<ITagsRepository>();
        _ = _tagsRepositoryMock.Setup(x => x.GetTagsByNames(It.IsAny<string[]>())).ReturnsAsync([]);
    }

    [Fact]
    public async Task Given_IncorrectData_When_UpdatingRecipe_ThenThrowsException()
    {
        string recipeName = "Test123";
        var mockExistingRecipe = new Recipe(recipeName, "desc", "long", [], new Statistics(), "imagePath");
        _ = _recipesRepositoryMock.Setup(x => x.GetById(1)).ReturnsAsync(mockExistingRecipe);

        string invalidBase64Image = "INVALID_BASE64";
        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), []);

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        var recipeUpdateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = recipeName,
            Ingredients = ["chicken", "rice", "salt"],
            Image = invalidBase64Image,
            PreparationDescription = "",
            TimeToPrepare = "medium"
        };

        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);
        Func<Task<Recipe>> action = async () => await sut.Update(recipeUpdateRequest, "");

        _ = await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Given_InvalidData_When_DeletingRecipe_ThenThrowsException()
    {
        int nonExistingRecipeId = 999;

        _ = _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).ThrowsAsync(new Exception("Recipe not found"));
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        _ = await Assert.ThrowsAsync<Exception>(() => sut.Delete(nonExistingRecipeId));

        _recipesRepositoryMock.Verify(x => x.Delete(nonExistingRecipeId), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidRecipeData_When_Updating_ThenThrowsInvalidDataException()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        string invalidBase64Image = "INVALID_IMAGE_STRING";
        string recipeName = "ExistingRecipe";

        var updateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = "",
            Ingredients = [],
            Image = invalidBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        string imagesDirectory = "some directory";

        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), []);
        var mockExistingRecipe = new Recipe(recipeName, "desc", "long", [], new Statistics(), "imagePath");

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        _ = _recipesRepositoryMock.Setup(x => x.GetByName(recipeName)).ReturnsAsync(mockExistingRecipe);

        _ = await Assert.ThrowsAsync<NotFoundException>(() => sut.Update(updateRequest, imagesDirectory));

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task Given_NonExistentRecipe_When_Updating_ThenThrowsNotFoundException()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        string validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        string nonexistentRecipeName = "NonexistentRecipe";

        var updateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = nonexistentRecipeName,
            Ingredients = ["chicken", "rice", "salt"],
            Image = validBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        string imagesDirectory = "some directory";

        _ = _recipesRepositoryMock.Setup<Task<Recipe?>>(x => x.GetByName(nonexistentRecipeName)).ReturnsAsync((Recipe?)null);
        _ = await Assert.ThrowsAsync<NotFoundException>(() => sut.Update(updateRequest, imagesDirectory));

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task Given_ValidData_When_CreatingRecipe_ThenSucceeds()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        string validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";

        var recipeRequest = new RecipeRequest()
        {
            Name = "Test123",
            Ingredients = ["boczus", "jajeczka", "maslo"],
            Image = validBase64Image,
            PreparationDescription = "124asfafafas",
            TimeToPrepare = "short"
        };

        string imagesDirectory = "some directory";
        string expectedImagePath = "some path";

        _recipesRepositoryMock.Setup(x => x.Create(It.IsAny<Recipe>())).Returns(Task.CompletedTask).Verifiable();
        _recipesRepositoryMock.Setup(x => x.SaveImage(It.IsAny<ImageInfo>())).ReturnsAsync(expectedImagePath).Verifiable();
        Recipe result = await sut.Create(recipeRequest, imagesDirectory);

        _ = result.Should().NotBeNull();
        _ = result.Name.Should().NotBeNullOrWhiteSpace().And.Be("Test123");
        _ = result.Ingredients.Should().HaveCount(3);
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
            [
                    new Ingredient("old ingredient1"),
                new Ingredient("old ingredient2"),
                new Ingredient("old ingredient3"),
                new Ingredient("old ingredient4"),
            ],
            new Statistics(),
            "oldimage.png"
            );

        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), []);
        var mockRecipePlanOfMeals = new RecipePlanOfMeals(existingRecipe, mockPlanOfMeals, DateTime.UtcNow);
        existingRecipe.RecipePlanOfMeals.Add(mockRecipePlanOfMeals);

        _ = _recipesRepositoryMock.Setup(x => x.GetByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        _ = _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Returns(Task.CompletedTask);

        await sut.Delete(existingRecipe.Id);

        _recipesRepositoryMock.Verify(x => x.Delete(existingRecipe.Id), Times.Once);
    }

    [Fact]
    public async Task Given_ValidUpdateData_When_UpdatingRecipe_ThenSucceeds()
    {
        var sut = new RecipeService(_recipesRepositoryMock.Object, _tagsRepositoryMock.Object, _loggerMock.Object);

        string validBase64Image = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        string oldRecipeName = "Test123";
        string newRecipeName = "UpdatedTest123";

        var mockRecipePlanOfMealsList = new List<RecipePlanOfMeals>();
        var mockPlanOfMeals = new PlanOfMeals("SomePlan", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), []);
        var mockExistingRecipe = new Recipe(oldRecipeName, "desc", "long", [], new Statistics(), "imagePath");

        mockRecipePlanOfMealsList.Add(new RecipePlanOfMeals(mockExistingRecipe, mockPlanOfMeals, DateTime.UtcNow));

        var recipeUpdateRequest = new UpdateRecipeRequest()
        {
            Id = 1,
            Name = newRecipeName,
            Ingredients = ["chicken", "rice", "salt"],
            Image = validBase64Image,
            PreparationDescription = "Updated Description",
            TimeToPrepare = "medium"
        };

        string imagesDirectory = "some updated directory";

        _ = _recipesRepositoryMock.Setup(x => x.GetById(1)).ReturnsAsync(mockExistingRecipe);
        _recipesRepositoryMock.Setup(x => x.SaveImage(It.IsAny<ImageInfo>())).ReturnsAsync("some updated path").Verifiable();
        _recipesRepositoryMock.Setup(x => x.Update(It.IsAny<Recipe>())).Returns(Task.CompletedTask).Verifiable();

        Recipe result = await sut.Update(recipeUpdateRequest, imagesDirectory);

        _ = result.Should().NotBeNull();
        _ = result.Name.Should().NotBeNullOrWhiteSpace().And.Be(newRecipeName);
        _ = result.Ingredients.Should().HaveCount(3);
        Assert.Equal("some updated path", result.Image);
        Assert.Equal(recipeUpdateRequest.Name, result.Name);
        Assert.Equal(recipeUpdateRequest.PreparationDescription, result.Description);
        Assert.Equal(recipeUpdateRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(recipeUpdateRequest.Ingredients.Count, result.Ingredients.Count);

        _recipesRepositoryMock.Verify(x => x.Update(It.IsAny<Recipe>()), Times.Once);
    }
}