using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedConfigAnotherOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingList");

            migrationBuilder.CreateTable(
                name: "RecipePlanOfMeals",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    PlanOfMealsId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipePlanOfMeals", x => new { x.RecipeId, x.PlanOfMealsId });
                    table.ForeignKey(
                        name: "FK_RecipePlanOfMeals_PlanOfMeals_PlanOfMealsId",
                        column: x => x.PlanOfMealsId,
                        principalTable: "PlanOfMeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipePlanOfMeals_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipePlanOfMeals_PlanOfMealsId",
                table: "RecipePlanOfMeals",
                column: "PlanOfMealsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipePlanOfMeals");

            migrationBuilder.CreateTable(
                name: "ShoppingList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingList_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingList_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingList_RecipeId",
                table: "ShoppingList",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingList_UserId",
                table: "ShoppingList",
                column: "UserId");
        }
    }
}
