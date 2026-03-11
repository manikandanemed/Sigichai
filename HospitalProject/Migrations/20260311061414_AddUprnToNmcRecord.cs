using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUprnToNmcRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UprnNumber",
                table: "NmcDoctorRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UprnNumber",
                table: "NmcDoctorRecords");
        }
    }
}
