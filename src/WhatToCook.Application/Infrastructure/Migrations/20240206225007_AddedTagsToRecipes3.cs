using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTagsToRecipes3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeTag",
                table: "RecipeTag");

            migrationBuilder.DropIndex(
                name: "IX_RecipeTag_RecipeId",
                table: "RecipeTag");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecipeTag");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeTag",
                table: "RecipeTag",
                columns: new[] { "RecipeId", "TagId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeTag",
                table: "RecipeTag");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecipeTag",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeTag",
                table: "RecipeTag",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_RecipeId",
                table: "RecipeTag",
                column: "RecipeId");
        }
    }
}
