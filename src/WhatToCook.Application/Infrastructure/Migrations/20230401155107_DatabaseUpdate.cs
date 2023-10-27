using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanOfmeals_Recipes_RecipeId",
                table: "PlanOfmeals");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanOfmeals_User_UserId",
                table: "PlanOfmeals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanOfmeals",
                table: "PlanOfmeals");

            migrationBuilder.DropIndex(
                name: "IX_Favourite_RecipeId_UserId",
                table: "Favourite");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "User");

            migrationBuilder.RenameTable(
                name: "PlanOfmeals",
                newName: "PlanOfMeals");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "User",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "FromDateToDate",
                table: "PlanOfMeals",
                newName: "ToDate");

            migrationBuilder.RenameIndex(
                name: "IX_PlanOfmeals_UserId",
                table: "PlanOfMeals",
                newName: "IX_PlanOfMeals_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanOfmeals_RecipeId",
                table: "PlanOfMeals",
                newName: "IX_PlanOfMeals_RecipeId");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Recipes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "PlanOfMeals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanOfMeals",
                table: "PlanOfMeals",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_RecipeId",
                table: "Favourite",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanOfMeals_Recipes_RecipeId",
                table: "PlanOfMeals",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanOfMeals_User_UserId",
                table: "PlanOfMeals",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanOfMeals_Recipes_RecipeId",
                table: "PlanOfMeals");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanOfMeals_User_UserId",
                table: "PlanOfMeals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanOfMeals",
                table: "PlanOfMeals");

            migrationBuilder.DropIndex(
                name: "IX_Favourite_RecipeId",
                table: "Favourite");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "PlanOfMeals");

            migrationBuilder.RenameTable(
                name: "PlanOfMeals",
                newName: "PlanOfmeals");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "User",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "PlanOfmeals",
                newName: "FromDateToDate");

            migrationBuilder.RenameIndex(
                name: "IX_PlanOfMeals_UserId",
                table: "PlanOfmeals",
                newName: "IX_PlanOfmeals_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanOfMeals_RecipeId",
                table: "PlanOfmeals",
                newName: "IX_PlanOfmeals_RecipeId");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanOfmeals",
                table: "PlanOfmeals",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_RecipeId_UserId",
                table: "Favourite",
                columns: new[] { "RecipeId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlanOfmeals_Recipes_RecipeId",
                table: "PlanOfmeals",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanOfmeals_User_UserId",
                table: "PlanOfmeals",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
