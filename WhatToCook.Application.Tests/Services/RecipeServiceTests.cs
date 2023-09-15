using Microsoft.Extensions.Logging;
using WhatToCook.Application.DataTransferObjects.Requests;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.Application.Services;

namespace WhatToCook.Application.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRecipesRepository> _recipesRepositoryMock;

    public RecipeServiceTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
    }
    [Fact]
    public async Task Given_ValidData_When_CreatingRecipe_ThenSucceeds()
    {
        //Arrange       
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);
        var recipeRequest = new RecipeRequest()
        {
            Name = "Test123",
            Ingredients = new List<string> { "boczus", "jajeczka", "maslo" },
            Image = "asasfasfas",
            PreparationDescription = "124asfafafas",
            TimeToPrepare = "short"
        };
        var imagesDirectory = "some directory";
        var expectedImagePath = "some path";

        //Act
        _recipesRepositoryMock.Setup(x => x.Create(It.IsAny<Recipe>())).Verifiable(Times.Once);
        _recipesRepositoryMock.Setup(x => x.SaveImage(recipeRequest.Image, imagesDirectory)).Returns(expectedImagePath);
        var result = await sut.Create(recipeRequest, imagesDirectory);

        //Assert
        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be("Test123");
        result.Ingredients.Should().HaveCount(3);
        Assert.Equal(expectedImagePath, result.Image);
        Assert.Equal(recipeRequest.Name, result.Name);
        Assert.Equal(recipeRequest.PreparationDescription, result.Description);
        Assert.Equal(recipeRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(recipeRequest.Ingredients.Count(), result.Ingredients.Count);
        _recipesRepositoryMock.Verify();
    }
    [Fact]
    public async Task Given_ValidData_When_UpdatingRecipe_ThenSucceeds()
    {
        //Arrange
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
            null,      
            "old image",       
            null 
            );
        //setup GetRecipeByName method to return existing recipe

        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        _recipesRepositoryMock.Setup(x => x.Update(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);
        var updateRecipeRequest = new UpdateRecipeRequest()
        {
            Name = "Test1236",
            Ingredients = new List<string> { "newboczus", "newjajeczka", "newmaslo", "gruszka" },
            Image = "new image",
            PreparationDescription = "newdescription",
            TimeToPrepare = "short"
        };
        var imagesDirectory = "some directory";
        var expectedImagePath = "some path";

        //Act
        _recipesRepositoryMock.Setup(x => x.SaveImage(updateRecipeRequest.Image, imagesDirectory)).Returns(expectedImagePath);
        var result = await sut.Update(updateRecipeRequest, imagesDirectory);

        //Assert
        result.Should().NotBeNull();
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be("Test1236");
        result.Ingredients.Should().HaveCount(4);
        Assert.Equal(expectedImagePath, result.Image);
        Assert.Equal(updateRecipeRequest.Name, result.Name);
        Assert.Equal(updateRecipeRequest.PreparationDescription, result.Description);
        Assert.Equal(updateRecipeRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(updateRecipeRequest.Ingredients.Count(), result.Ingredients.Count);
        _recipesRepositoryMock.Verify();
    }
    [Fact]
    public async Task Given_ValidData_When_UpdatingRecipe2_ThenSucceeds()
    {
        //Arrange

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
           null,
           "old image",
           null
           );
        //setup GetRecipeByName method to return existing recipe
        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);
        var updateRecipeRequest = new UpdateRecipeRequest()
        {
            Name = "Test1236",
            Ingredients = new List<string> { "newboczus", "newjajeczka", "newmaslo", "gruszka" },
            Image = "new image",
            PreparationDescription = "newdescription",
            TimeToPrepare = "short"
        };
        var imagesDirectory = "some directory";
        var expectedImagePath = "some path";
        _recipesRepositoryMock.Setup(x => x.Update(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        _recipesRepositoryMock.Setup(x => x.SaveImage(updateRecipeRequest.Image, imagesDirectory)).Returns(expectedImagePath);
        //Act
        var result = await sut.Update(updateRecipeRequest, imagesDirectory);

        //Assert
        _recipesRepositoryMock.Verify(x => x.Update(It.Is<Recipe>(r =>
            r.Name == updateRecipeRequest.Name &&
            r.Description == updateRecipeRequest.PreparationDescription &&
            r.TimeToPrepare == updateRecipeRequest.TimeToPrepare &&
            r.Image == result.Image &&
            r.Ingredients.Count == updateRecipeRequest.Ingredients.Count)), Times.Once);
    }
    [Fact]
    public async Task Given_InvalidData_When_UpdatingRecipe_ThenThrowsException()
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
           null,
           "old image",
           null
           );
        //setup GetRecipeByName method to return existing recipe
        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);
        var updateRecipeRequest = new UpdateRecipeRequest()
        {
            Name = " ",
            Ingredients = new List<string> { "newboczus", "newjajeczka", "newmaslo", "gruszka" },
            Image = "new image",
            PreparationDescription = "newdescription",
            TimeToPrepare = "short"
        };
        var imagesDirectory = "some directory";
        var expectedImagePath = "some path";
        _recipesRepositoryMock.Setup(x => x.Update(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        _recipesRepositoryMock.Setup(x => x.SaveImage(updateRecipeRequest.Image, imagesDirectory)).Returns(expectedImagePath);

        //Assert
        await Assert.ThrowsAsync<Exception>(() => sut.Update(updateRecipeRequest, imagesDirectory));
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
            null,
            "old image",
            null
            );

        //setup GetRecipeByName method to return existing recipe
        _recipesRepositoryMock.Setup(x => x.GetRecipeByName(It.IsAny<string>())).ReturnsAsync(existingRecipe);
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);

        //setup Delete method to delete the recipe
        _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).Returns(Task.CompletedTask);

        //Act
        await sut.Delete(existingRecipe.Id);

        //Assert
        _recipesRepositoryMock.Verify(x => x.Delete(existingRecipe.Id), Times.Once);
    }

    [Fact]
    public async Task Given_InvalidData_When_DeletingRecipe_ThenThrowsException()
    {
        var nonExistingRecipeId = 999;

        //setup Delete method to throw an exception
        _recipesRepositoryMock.Setup(x => x.Delete(It.IsAny<int>())).ThrowsAsync(new Exception("Recipe not found"));
        var loggerMock = new Mock<ILogger<RecipeService>>();
        var sut = new RecipeService(_recipesRepositoryMock.Object, loggerMock.Object);

        //Act and Assert
        await Assert.ThrowsAsync<Exception>(() => sut.Delete(nonExistingRecipeId));

        _recipesRepositoryMock.Verify(x => x.Delete(nonExistingRecipeId), Times.Once);
    }
}

