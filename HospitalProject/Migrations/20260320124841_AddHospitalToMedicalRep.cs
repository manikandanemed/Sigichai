using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalToMedicalRep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "MedicalRepSlots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "MedicalRepAppointments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepSlots_HospitalId",
                table: "MedicalRepSlots",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepAppointments_HospitalId",
                table: "MedicalRepAppointments",
                column: "HospitalId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRepAppointments_Hospitals_HospitalId",
                table: "MedicalRepAppointments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRepSlots_Hospitals_HospitalId",
                table: "MedicalRepSlots",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRepAppointments_Hospitals_HospitalId",
                table: "MedicalRepAppointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRepSlots_Hospitals_HospitalId",
                table: "MedicalRepSlots");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRepSlots_HospitalId",
                table: "MedicalRepSlots");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRepAppointments_HospitalId",
                table: "MedicalRepAppointments");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "MedicalRepSlots");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "MedicalRepAppointments");
        }
    }
}
