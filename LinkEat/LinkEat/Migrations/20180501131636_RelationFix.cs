using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LinkEat.Migrations
{
    public partial class RelationFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vegetables_Meals_MealId",
                table: "Vegetables");

            migrationBuilder.DropIndex(
                name: "IX_Vegetables_MealId",
                table: "Vegetables");

            migrationBuilder.DropColumn(
                name: "MealId",
                table: "Vegetables");

            migrationBuilder.CreateTable(
                name: "MealVegetable",
                columns: table => new
                {
                    MealId = table.Column<int>(nullable: false),
                    VegetableId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealVegetable", x => new { x.MealId, x.VegetableId });
                    table.ForeignKey(
                        name: "FK_MealVegetable_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealVegetable_Vegetables_VegetableId",
                        column: x => x.VegetableId,
                        principalTable: "Vegetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealVegetable_VegetableId",
                table: "MealVegetable",
                column: "VegetableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealVegetable");

            migrationBuilder.AddColumn<int>(
                name: "MealId",
                table: "Vegetables",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vegetables_MealId",
                table: "Vegetables",
                column: "MealId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vegetables_Meals_MealId",
                table: "Vegetables",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
