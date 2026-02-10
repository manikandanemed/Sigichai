using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRepAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicalReps_UserId",
                table: "MedicalReps");

            migrationBuilder.CreateTable(
                name: "MedicalRepAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicalRepId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeSlot = table.Column<string>(type: "text", nullable: false),
                    TempToken = table.Column<string>(type: "text", nullable: false),
                    QueueToken = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRepAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRepAppointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRepAppointments_MedicalReps_MedicalRepId",
                        column: x => x.MedicalRepId,
                        principalTable: "MedicalReps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReps_UserId",
                table: "MedicalReps",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepAppointments_DoctorId",
                table: "MedicalRepAppointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepAppointments_MedicalRepId",
                table: "MedicalRepAppointments",
                column: "MedicalRepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRepAppointments");

            migrationBuilder.DropIndex(
                name: "IX_MedicalReps_UserId",
                table: "MedicalReps");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReps_UserId",
                table: "MedicalReps",
                column: "UserId");
        }
    }
}
