using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Application.Infrastracture.Migrations
{
    /// <inheritdoc />
    public partial class LaptopikMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanOfMeals_Recipes_RecipeId",
                table: "PlanOfMeals");

            migrationBuilder.DropIndex(
                name: "IX_PlanOfMeals_RecipeId",
                table: "PlanOfMeals");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "PlanOfMeals");

            migrationBuilder.CreateTable(
                name: "PlanOfMealsRecipe",
                columns: table => new
                {
                    PlansOfMealsId = table.Column<int>(type: "integer", nullable: false),
                    RecipesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanOfMealsRecipe", x => new { x.PlansOfMealsId, x.RecipesId });
                    table.ForeignKey(
                        name: "FK_PlanOfMealsRecipe_PlanOfMeals_PlansOfMealsId",
                        column: x => x.PlansOfMealsId,
                        principalTable: "PlanOfMeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanOfMealsRecipe_Recipes_RecipesId",
                        column: x => x.RecipesId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanOfMealsRecipe_RecipesId",
                table: "PlanOfMealsRecipe",
                column: "RecipesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanOfMealsRecipe");

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "PlanOfMeals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlanOfMeals_RecipeId",
                table: "PlanOfMeals",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanOfMeals_Recipes_RecipeId",
                table: "PlanOfMeals",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
