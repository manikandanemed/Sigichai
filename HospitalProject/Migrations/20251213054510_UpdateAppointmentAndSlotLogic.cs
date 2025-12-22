using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentAndSlotLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_FamilyMembers_FamilyMemberId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_FamilyMemberId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "FamilyMemberId",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "DoctorAvailabilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FamilyMemberId",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_FamilyMemberId",
                table: "Appointments",
                column: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_FamilyMembers_FamilyMemberId",
                table: "Appointments",
                column: "FamilyMemberId",
                principalTable: "FamilyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
