namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class LargeSeedWebApplicationFactory : TestWebApplicationFactory
{
    protected override bool EnableSeedData => true;
    protected override string? SeedDataProfile => "large";
}
