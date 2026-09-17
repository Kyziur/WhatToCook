using Aspire.Hosting.Docker;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("compose").WithDashboard(false);

// The existing local volume contains a PostgreSQL 17 cluster. Keep its compatible
// image and mount layout until an explicit PostgreSQL major-version migration is run.
var postgres = builder
    .AddPostgres("what-to-cook-postgres")
    .WithImageTag("17.6")
    .WithPgAdmin()
    .WithDataVolume();

var recipeDb = postgres.AddDatabase("whattocook-db");

var api = builder
    .AddProject<Projects.WhatToCook_Api>("api")
    .WithEnvironment("Storage__RecipeImagesPath", "/data/recipe-images")
    .WithReference(recipeDb)
    .WaitFor(postgres);

builder
    .AddProject<Projects.WhatToCook_Web>("web")
    .WaitFor(api)
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
