using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedConfig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_Name",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Name",
                table: "Recipes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_Name",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Name",
                table: "Recipes",
                column: "Name");
        }
    }
}
