namespace WhatToCook.Api.IntegrationTests.Specs;

public sealed class SeedEnabledWebApplicationFactory : TestWebApplicationFactory
{
    protected override bool EnableSeedData => true;
}
