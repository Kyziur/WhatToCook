using Bunit;
using Microsoft.AspNetCore.Components;
using WhatToCook.Web.Components.Features.Planning.Components;

namespace WhatToCook.Web.ComponentTests.Specs;

public sealed class PlannerMealCardSpecificationTests
{
    [Fact]
    public void CompactCard_ShouldKeepTitleAndMultiplierWithoutPhoto()
    {
        using var context = new BunitContext();
        var cut = context.Render<PlannerMealCard>(parameters =>
            parameters
                .Add(x => x.Title, "Zupa pomidorowa")
                .Add(x => x.Multiplier, 1.5m)
                .Add(x => x.PhotoPath, "test.jpg")
                .Add(x => x.Compact, true)
                .Add(x => x.Disabled, true)
        );

        Assert.Contains("Zupa pomidorowa", cut.Markup);
        Assert.Contains("1.5 × porcja", cut.Markup);
        Assert.Empty(cut.FindAll("img"));
        Assert.True(cut.Find(".planner-meal-menu-toggle").HasAttribute("disabled"));
    }

    [Fact]
    public void ExpandedCard_ShouldDelegateActionsToThePage()
    {
        using var context = new BunitContext();
        var toggled = 0;
        var decreased = 0;
        var increased = 0;
        var removed = 0;

        var cut = context.Render<PlannerMealCard>(parameters =>
            parameters
                .Add(x => x.Title, "Makaron")
                .Add(x => x.Multiplier, 2m)
                .Add(x => x.ActionsOpen, true)
                .Add(x => x.OnToggleActions, EventCallback.Factory.Create(this, () => toggled++))
                .Add(x => x.OnDecrease, EventCallback.Factory.Create(this, () => decreased++))
                .Add(x => x.OnIncrease, EventCallback.Factory.Create(this, () => increased++))
                .Add(x => x.OnRemove, EventCallback.Factory.Create(this, () => removed++))
        );

        Assert.Contains("planner-meal-card-expanded", cut.Markup);
        Assert.Equal("true", cut.Find(".planner-meal-menu-toggle").GetAttribute("aria-expanded"));

        cut.Find(".planner-meal-menu-toggle").Click();
        cut.Find("button[aria-label='Zmniejsz mnożnik Makaron']").Click();
        cut.Find("button[aria-label='Zwiększ mnożnik Makaron']").Click();
        cut.Find("button[aria-label='Usuń Makaron']").Click();

        Assert.Equal(1, toggled);
        Assert.Equal(1, decreased);
        Assert.Equal(1, increased);
        Assert.Equal(1, removed);
    }
}
