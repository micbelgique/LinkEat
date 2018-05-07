using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LinkEat.Migrations
{
    public partial class AddedSlackIdForUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "Orders",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "SlackId",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlackId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Orders",
                newName: "date");
        }
    }
}
