using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KafePersonalTid.Migrations
{
    /// <inheritdoc />
    public partial class RenameHoursToHoursWorked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Hours",
                table: "WorkEntries",
                newName: "HoursWorked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HoursWorked",
                table: "WorkEntries",
                newName: "Hours");
        }
    }
}
