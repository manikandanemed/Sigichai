using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalRepSlotCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "MedicalRepSlots");

            migrationBuilder.AddColumn<int>(
                name: "BookedCount",
                table: "MedicalRepSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxReps",
                table: "MedicalRepSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedCount",
                table: "MedicalRepSlots");

            migrationBuilder.DropColumn(
                name: "MaxReps",
                table: "MedicalRepSlots");

            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "MedicalRepSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
