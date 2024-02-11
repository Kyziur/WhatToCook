using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTagsToRecipes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Recipes_RecipesId",
                table: "RecipeTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Tag_TagId",
                table: "RecipeTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Tag_TagsId",
                table: "RecipeTag");

            migrationBuilder.DropIndex(
                name: "IX_RecipeTag_RecipesId",
                table: "RecipeTag");

            migrationBuilder.DropIndex(
                name: "IX_RecipeTag_TagsId",
                table: "RecipeTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "RecipesId",
                table: "RecipeTag");

            migrationBuilder.DropColumn(
                name: "TagsId",
                table: "RecipeTag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Tags_TagId",
                table: "RecipeTag",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeTag_Tags_TagId",
                table: "RecipeTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.AddColumn<int>(
                name: "RecipesId",
                table: "RecipeTag",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TagsId",
                table: "RecipeTag",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_RecipesId",
                table: "RecipeTag",
                column: "RecipesId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeTag_TagsId",
                table: "RecipeTag",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Recipes_RecipesId",
                table: "RecipeTag",
                column: "RecipesId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Tag_TagId",
                table: "RecipeTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeTag_Tag_TagsId",
                table: "RecipeTag",
                column: "TagsId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
