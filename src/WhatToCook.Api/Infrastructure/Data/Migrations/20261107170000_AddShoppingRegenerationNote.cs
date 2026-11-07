using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatToCook.Api.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261107170000_AddShoppingRegenerationNote")]
public partial class AddShoppingRegenerationNote : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RegenerationNote",
            table: "shopping_list_items",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RegenerationNote",
            table: "shopping_list_items"
        );
    }
}
