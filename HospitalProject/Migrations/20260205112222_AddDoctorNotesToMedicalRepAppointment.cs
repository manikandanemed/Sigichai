using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    public partial class AddDoctorNotesToMedicalRepAppointment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ ONLY ADD COLUMN
            migrationBuilder.AddColumn<string>(
                name: "DoctorNotes",
                table: "MedicalRepAppointments",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ONLY REMOVE COLUMN
            migrationBuilder.DropColumn(
                name: "DoctorNotes",
                table: "MedicalRepAppointments");
        }
    }
}
