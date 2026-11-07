using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Api.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261107162000_AddShoppingQuantityContext")]
public partial class AddShoppingQuantityContext : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "QuantityContext",
            table: "shopping_list_items",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "QuantityContext",
            table: "shopping_list_items"
        );
    }
}
