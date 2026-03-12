using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAvailabilitySpeciality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorAvailabilitySpecialities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorAvailabilityId = table.Column<int>(type: "integer", nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilitySpecialities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilitySpecialities_DoctorAvailabilities_DoctorA~",
                        column: x => x.DoctorAvailabilityId,
                        principalTable: "DoctorAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilitySpecialities_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySpecialities_DoctorAvailabilityId",
                table: "DoctorAvailabilitySpecialities",
                column: "DoctorAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySpecialities_SpecialityId",
                table: "DoctorAvailabilitySpecialities",
                column: "SpecialityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorAvailabilitySpecialities");
        }
    }
}
