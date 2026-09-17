using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeImportDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recipe_import_drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: true
                    ),
                    Servings = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                    FinalizedRecipeId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_recipe_import_drafts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_import_ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
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
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_import_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_import_ingredients_recipe_import_drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "recipe_import_drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_import_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldPath = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Code = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    Message = table.Column<string>(
                        type: "character varying(1000)",
                        maxLength: 1000,
                        nullable: false
                    ),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_import_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_import_issues_recipe_import_drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "recipe_import_drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_import_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(
                        type: "character varying(4000)",
                        maxLength: 4000,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_import_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_import_steps_recipe_import_drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "recipe_import_drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "recipe_import_tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_import_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_import_tags_recipe_import_drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "recipe_import_drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_drafts_SourceType",
                table: "recipe_import_drafts",
                column: "SourceType"
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_drafts_Status",
                table: "recipe_import_drafts",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_ingredients_DraftId_SortOrder",
                table: "recipe_import_ingredients",
                columns: new[] { "DraftId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_issues_DraftId_SortOrder",
                table: "recipe_import_issues",
                columns: new[] { "DraftId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_steps_DraftId_SortOrder",
                table: "recipe_import_steps",
                columns: new[] { "DraftId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_recipe_import_tags_DraftId_SortOrder",
                table: "recipe_import_tags",
                columns: new[] { "DraftId", "SortOrder" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "recipe_import_ingredients");

            migrationBuilder.DropTable(name: "recipe_import_issues");

            migrationBuilder.DropTable(name: "recipe_import_steps");

            migrationBuilder.DropTable(name: "recipe_import_tags");

            migrationBuilder.DropTable(name: "recipe_import_drafts");
        }
    }
}
