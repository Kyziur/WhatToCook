using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WhatToCook.Application.Domain;
using WhatToCook.Application.Infrastructure.Configurations;

namespace WhatToCook.Application.Infrastructure;

public class DatabaseContext : DbContext
{
    private readonly string _connectionString;
    private readonly bool _isDevelopment;

    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration, IHostEnvironment environment)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string is required");
        _isDevelopment = environment.IsDevelopment();
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<PlanOfMeals> PlanOfMeals => Set<PlanOfMeals>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseNpgsql(_connectionString);
        if (_isDevelopment)
        {
            _ = optionsBuilder.EnableDetailedErrors().EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeEntityConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}