using Bunit;
using Microsoft.AspNetCore.Components;
using WhatToCook.Web.Components.Features.Recipes.Shared;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class RecipeTagChipsSpecificationTests
{
    [Fact]
    public void SharedTags_ShouldRenderAccessibleButtonsAndForwardRemovedValue()
    {
        using var context = new BunitContext();
        var removed = new List<string>();
        var cut = context.Render<RecipeTagChips>(parameters =>
            parameters
                .Add(x => x.Tags, new[] { "Obiad", "Szybkie" })
                .Add(
                    x => x.OnRemove,
                    EventCallback.Factory.Create<string>(this, (string tag) => removed.Add(tag))
                )
        );

        Assert.Equal(2, cut.FindAll(".chip.selected-chip").Count);
        cut.Find("button[aria-label='Usuń tag Szybkie']").Click();
        Assert.Equal("Szybkie", Assert.Single(removed));
    }

    [Fact]
    public void EmptyTags_ShouldNotRenderTagContainer()
    {
        using var context = new BunitContext();
        var cut = context.Render<RecipeTagChips>(parameters =>
            parameters.Add(x => x.Tags, Array.Empty<string>())
        );

        Assert.Empty(cut.FindAll(".chip-row"));
    }
}
