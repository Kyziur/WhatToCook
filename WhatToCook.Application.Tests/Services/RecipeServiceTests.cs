using Moq;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Repositories;
using WhatToCook.Application.Services;
using WhatToCook.WebApp.DataTransferObject.Requests;

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
        _recipesRepositoryMock.Setup(x => x.Create(It.IsAny<Recipe>())).Verifiable(Times.Once);
        var sut = new RecipeService(_recipesRepositoryMock.Object);
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
        _recipesRepositoryMock.Setup(x => x.SaveImage(recipeRequest.Image, imagesDirectory)).Returns(expectedImagePath);
        var result = await sut.Create(recipeRequest, imagesDirectory);

        //Assert
        _recipesRepositoryMock.Verify();
        Assert.Equal(expectedImagePath, result.Image);
        Assert.Equal(recipeRequest.Name, result.Name);
        Assert.Equal(recipeRequest.PreparationDescription, result.Description);
        Assert.Equal(recipeRequest.TimeToPrepare, result.TimeToPrepare);
        Assert.Equal(recipeRequest.Ingredients.Count(), result.Ingredients.Count);
    }
}
