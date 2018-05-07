using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LinkEat.Migrations
{
    public partial class AddedVegetablesAndMealPerPlaceConcepts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Crudites",
                table: "Meals");

            migrationBuilder.RenameColumn(
                name: "Sauce",
                table: "Meals",
                newName: "SauceId");

            migrationBuilder.AddColumn<string>(
                name: "OrderingAPI",
                table: "Places",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                table: "Orders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlaceId",
                table: "Meals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Vegetables",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    MealId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vegetables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vegetables_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vegetables_MealId",
                table: "Vegetables",
                column: "MealId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vegetables");

            migrationBuilder.DropColumn(
                name: "OrderingAPI",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Confirmed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Meals");

            migrationBuilder.RenameColumn(
                name: "SauceId",
                table: "Meals",
                newName: "Sauce");

            migrationBuilder.AddColumn<bool>(
                name: "Crudites",
                table: "Meals",
                nullable: false,
                defaultValue: false);
        }
    }
}
