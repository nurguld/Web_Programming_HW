using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineClub.Migrations.CineDb
{
    /// <inheritdoc />
    public partial class AddUpdateTimeToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Reviews",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Reviews");
        }
    }
}
