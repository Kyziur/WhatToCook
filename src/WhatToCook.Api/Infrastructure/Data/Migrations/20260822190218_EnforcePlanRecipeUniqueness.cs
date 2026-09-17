using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePlanRecipeUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_RecipeId",
                table: "planned_recipes"
            );

            migrationBuilder.CreateIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_RecipeId",
                table: "planned_recipes",
                columns: new[] { "MealPlanId", "PlannedDate", "RecipeId" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_RecipeId",
                table: "planned_recipes"
            );

            migrationBuilder.CreateIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_RecipeId",
                table: "planned_recipes",
                columns: new[] { "MealPlanId", "PlannedDate", "RecipeId" }
            );
        }
    }
}
