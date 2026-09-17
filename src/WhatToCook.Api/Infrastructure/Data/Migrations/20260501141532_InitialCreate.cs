using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    NormalizedName = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "meal_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    HasGeneratedShoppingList = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plans", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    NormalizedTitle = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Servings = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    MainPhotoPath = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    NormalizedName = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "shopping_lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_lists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shopping_lists_meal_plans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "planned_recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Multiplier = table.Column<decimal>(
                        type: "numeric(10,2)",
                        precision: 10,
                        scale: 2,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planned_recipes_meal_plans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_planned_recipes_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityText = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    Unit = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: true
                    ),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(
                        type: "character varying(4000)",
                        maxLength: 4000,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_steps_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_tags",
                columns: table => new
                {
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_tags", x => new { x.RecipeId, x.TagId });
                    table.ForeignKey(
                        name: "FK_recipe_tags_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_recipe_tags_tags_TagId",
                        column: x => x.TagId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "shopping_list_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    NormalizedName = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: true
                    ),
                    QuantityText = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    Unit = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: true
                    ),
                    State = table.Column<int>(type: "integer", nullable: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shopping_list_items_shopping_lists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "shopping_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ingredients_NormalizedName",
                table: "ingredients",
                column: "NormalizedName",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_meal_plans_DateFrom_DateTo",
                table: "meal_plans",
                columns: new[] { "DateFrom", "DateTo" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_RecipeId",
                table: "planned_recipes",
                columns: new[] { "MealPlanId", "PlannedDate", "RecipeId" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_planned_recipes_MealPlanId_PlannedDate_SortOrder",
                table: "planned_recipes",
                columns: new[] { "MealPlanId", "PlannedDate", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_planned_recipes_RecipeId",
                table: "planned_recipes",
                column: "RecipeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_IngredientId",
                table: "recipe_ingredients",
                column: "IngredientId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RecipeId_SortOrder",
                table: "recipe_ingredients",
                columns: new[] { "RecipeId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_steps_RecipeId_SortOrder",
                table: "recipe_steps",
                columns: new[] { "RecipeId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_tags_TagId",
                table: "recipe_tags",
                column: "TagId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipes_IsActive_Title",
                table: "recipes",
                columns: new[] { "IsActive", "Title" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipes_NormalizedTitle",
                table: "recipes",
                column: "NormalizedTitle",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_shopping_list_items_ShoppingListId_IsManual_NormalizedName_~",
                table: "shopping_list_items",
                columns: new[] { "ShoppingListId", "IsManual", "NormalizedName", "Unit" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_shopping_list_items_ShoppingListId_SortOrder",
                table: "shopping_list_items",
                columns: new[] { "ShoppingListId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_shopping_lists_MealPlanId",
                table: "shopping_lists",
                column: "MealPlanId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_shopping_lists_UpdatedAt",
                table: "shopping_lists",
                column: "UpdatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_tags_NormalizedName",
                table: "tags",
                column: "NormalizedName",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "planned_recipes");

            migrationBuilder.DropTable(name: "recipe_ingredients");

            migrationBuilder.DropTable(name: "recipe_steps");

            migrationBuilder.DropTable(name: "recipe_tags");

            migrationBuilder.DropTable(name: "shopping_list_items");

            migrationBuilder.DropTable(name: "ingredients");

            migrationBuilder.DropTable(name: "recipes");

            migrationBuilder.DropTable(name: "tags");

            migrationBuilder.DropTable(name: "shopping_lists");

            migrationBuilder.DropTable(name: "meal_plans");
        }
    }
}
