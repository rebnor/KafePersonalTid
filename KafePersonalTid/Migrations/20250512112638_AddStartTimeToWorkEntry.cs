using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KafePersonalTid.Migrations
{
    /// <inheritdoc />
    public partial class AddStartTimeToWorkEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "WorkEntries",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "WorkEntries");
        }
    }
}
