using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSlotDateToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableDate",
                table: "DoctorAvailabilities",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "AvailableDate",
                table: "DoctorAvailabilities",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
