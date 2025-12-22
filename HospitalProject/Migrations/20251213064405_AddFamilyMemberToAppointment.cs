using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyMemberToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_FamilyMembers_FamilyMemberId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_FamilyMemberId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "FamilyMemberId",
                table: "Appointments");
        }
    }
}
