using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KafePersonalTid.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyWageToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyWage",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyWage",
                table: "Employees");
        }
    }
}
