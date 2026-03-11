using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalPharmacies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PharmacyName = table.Column<string>(type: "text", nullable: false),
                    OwnerName = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    LicenseNumber = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    OffersHomeDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryRadius = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AverageRating = table.Column<double>(type: "double precision", precision: 3, scale: 2, nullable: false),
                    TotalRatings = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPharmacies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hospitals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Area = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalPharmacyStaffRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalPharmacyStaffRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GenericName = table.Column<string>(type: "text", nullable: false),
                    BrandName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmcDoctorRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FatherOrHusbandName = table.Column<string>(type: "text", nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: false),
                    DateOfRegistration = table.Column<DateOnly>(type: "date", nullable: false),
                    StateCouncil = table.Column<string>(type: "text", nullable: false),
                    Degree = table.Column<string>(type: "text", nullable: false),
                    GraduationYear = table.Column<int>(type: "integer", nullable: false),
                    University = table.Column<string>(type: "text", nullable: false),
                    PermanentAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmcDoctorRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpStores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    OtpCode = table.Column<string>(type: "text", nullable: false),
                    OtpType = table.Column<string>(type: "text", nullable: true),
                    Purpose = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpStores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specialities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalPharmacyDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalPharmacyId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPharmacyDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalPharmacyDocuments_ExternalPharmacies_ExternalPharma~",
                        column: x => x.ExternalPharmacyId,
                        principalTable: "ExternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalPharmacies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HospitalId = table.Column<int>(type: "integer", nullable: false),
                    PharmacyName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalPharmacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalPharmacies_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicineIdA = table.Column<int>(type: "integer", nullable: false),
                    MedicineIdB = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugInteractions_Medicines_MedicineIdA",
                        column: x => x.MedicineIdA,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrugInteractions_Medicines_MedicineIdB",
                        column: x => x.MedicineIdB,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    HospitalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Specialization = table.Column<string>(type: "text", nullable: false),
                    HospitalId = table.Column<int>(type: "integer", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsArrivedToday = table.Column<bool>(type: "boolean", nullable: false),
                    ArrivedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doctors_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Doctors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalReps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    Area = table.Column<string>(type: "text", nullable: false),
                    IdProofNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalReps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalReps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    BloodGroup = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    EmergencyContact = table.Column<string>(type: "text", nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    MedicalHistory = table.Column<string>(type: "text", nullable: true),
                    Allergies = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalPharmacyInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Barcode = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InternalPharmacyId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalPharmacyInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalPharmacyInventories_InternalPharmacies_InternalPhar~",
                        column: x => x.InternalPharmacyId,
                        principalTable: "InternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalPharmacyInventories_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    HospitalId = table.Column<int>(type: "integer", nullable: true),
                    AvailableDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeSlot = table.Column<string>(type: "text", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DoctorDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorDocuments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    FatherOrHusbandName = table.Column<string>(type: "text", nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    DateOfRegistration = table.Column<DateOnly>(type: "date", nullable: false),
                    StateCouncil = table.Column<string>(type: "text", nullable: false),
                    Degree = table.Column<string>(type: "text", nullable: false),
                    GraduationYear = table.Column<int>(type: "integer", nullable: false),
                    University = table.Column<string>(type: "text", nullable: false),
                    PermanentAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorServiceLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    HospitalId = table.Column<int>(type: "integer", nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorServiceLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorServiceLocations_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorServiceLocations_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorServiceLocations_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorStaffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StaffRole = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorStaffs_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorStaffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "text", nullable: false),
                    YearOfRegistration = table.Column<int>(type: "integer", nullable: false),
                    CouncilName = table.Column<string>(type: "text", nullable: false),
                    VerificationStatus = table.Column<string>(type: "text", nullable: false),
                    VerifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorVerifications_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRepSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    SlotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeSlot = table.Column<string>(type: "text", nullable: false),
                    MaxReps = table.Column<int>(type: "integer", nullable: false),
                    BookedCount = table.Column<int>(type: "integer", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRepSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRepSlots_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoctorNotes = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    Relationship = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    BloodGroup = table.Column<string>(type: "text", nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyMembers_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPreferredPharmacies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    ExternalPharmacyId = table.Column<int>(type: "integer", nullable: false),
                    SetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPreferredPharmacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPreferredPharmacies_ExternalPharmacies_ExternalPharm~",
                        column: x => x.ExternalPharmacyId,
                        principalTable: "ExternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientPreferredPharmacies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyNotifications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorServiceSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorServiceLocationId = table.Column<int>(type: "integer", nullable: false),
                    TimeSlot = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorServiceSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorServiceSlots_DoctorServiceLocations_DoctorServiceLoca~",
                        column: x => x.DoctorServiceLocationId,
                        principalTable: "DoctorServiceLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    DoctorServiceLocationId = table.Column<int>(type: "integer", nullable: true),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeSlot = table.Column<string>(type: "text", nullable: false),
                    TempToken = table.Column<string>(type: "text", nullable: false),
                    QueueToken = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Prescription = table.Column<string>(type: "text", nullable: true),
                    Fees = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentMode = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FamilyMemberId = table.Column<int>(type: "integer", nullable: true),
                    HospitalId = table.Column<int>(type: "integer", nullable: true),
                    ReasonForVisit = table.Column<string>(type: "text", nullable: true),
                    BloodPressure = table.Column<string>(type: "text", nullable: true),
                    Pulse = table.Column<int>(type: "integer", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: true),
                    SpO2 = table.Column<int>(type: "integer", nullable: true),
                    RazorpayOrderId = table.Column<string>(type: "text", nullable: true),
                    RazorpayPaymentId = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_DoctorServiceLocations_DoctorServiceLocationId",
                        column: x => x.DoctorServiceLocationId,
                        principalTable: "DoctorServiceLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Appointments_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    RazorpayOrderId = table.Column<string>(type: "text", nullable: false),
                    RazorpayPaymentId = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RawResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DoctorNotes = table.Column<string>(type: "text", nullable: true),
                    QRData = table.Column<string>(type: "text", nullable: true),
                    ValidityDays = table.Column<int>(type: "integer", nullable: false),
                    MaxRefills = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispenseRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    PharmacistId = table.Column<int>(type: "integer", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseRecords_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispenseRecords_Users_PharmacistId",
                        column: x => x.PharmacistId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    QuantityPrescribed = table.Column<int>(type: "integer", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false),
                    GenericSubstitutionAllowed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionQrCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    QrPayload = table.Column<string>(type: "text", nullable: false),
                    QrImageBase64 = table.Column<string>(type: "text", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxRefills = table.Column<int>(type: "integer", nullable: false),
                    UsedRefills = table.Column<int>(type: "integer", nullable: false),
                    IsFullyUsed = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionQrCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionQrCodes_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    PharmacyType = table.Column<string>(type: "text", nullable: false),
                    InternalPharmacyId = table.Column<int>(type: "integer", nullable: true),
                    ExternalPharmacyId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RoutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionRoutes_ExternalPharmacies_ExternalPharmacyId",
                        column: x => x.ExternalPharmacyId,
                        principalTable: "ExternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrescriptionRoutes_InternalPharmacies_InternalPharmacyId",
                        column: x => x.InternalPharmacyId,
                        principalTable: "InternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrescriptionRoutes_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispenseItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispenseRecordId = table.Column<int>(type: "integer", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<string>(type: "text", nullable: false),
                    QuantityDispensed = table.Column<int>(type: "integer", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseItems_DispenseRecords_DispenseRecordId",
                        column: x => x.DispenseRecordId,
                        principalTable: "DispenseRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispenseItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QrScanLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionQrCodeId = table.Column<int>(type: "integer", nullable: false),
                    ScannedByUserId = table.Column<int>(type: "integer", nullable: false),
                    PharmacyName = table.Column<string>(type: "text", nullable: false),
                    WasValid = table.Column<bool>(type: "boolean", nullable: false),
                    InvalidReason = table.Column<string>(type: "text", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefillNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrScanLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrScanLogs_PrescriptionQrCodes_PrescriptionQrCodeId",
                        column: x => x.PrescriptionQrCodeId,
                        principalTable: "PrescriptionQrCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QrScanLogs_Users_ScannedByUserId",
                        column: x => x.ScannedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionRouteId = table.Column<int>(type: "integer", nullable: false),
                    ExternalPharmacyId = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OffersDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    QuotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyQuotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyQuotations_ExternalPharmacies_ExternalPharmacyId",
                        column: x => x.ExternalPharmacyId,
                        principalTable: "ExternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyQuotations_PrescriptionRoutes_PrescriptionRouteId",
                        column: x => x.PrescriptionRouteId,
                        principalTable: "PrescriptionRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<string>(type: "text", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: true),
                    PaymentMode = table.Column<string>(type: "text", nullable: false),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyOrders_PharmacyQuotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "PharmacyQuotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyQuotationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationId = table.Column<int>(type: "integer", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityAvailable = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyQuotationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyQuotationItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyQuotationItems_PharmacyQuotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "PharmacyQuotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyOrderStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PharmacyOrderId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyOrderStatusLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyOrderStatusLogs_PharmacyOrders_PharmacyOrderId",
                        column: x => x.PharmacyOrderId,
                        principalTable: "PharmacyOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalPharmacyId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    PharmacyOrderId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Review = table.Column<string>(type: "text", nullable: true),
                    RatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyRatings_ExternalPharmacies_ExternalPharmacyId",
                        column: x => x.ExternalPharmacyId,
                        principalTable: "ExternalPharmacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PharmacyRatings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyRatings_PharmacyOrders_PharmacyOrderId",
                        column: x => x.PharmacyOrderId,
                        principalTable: "PharmacyOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_HospitalId",
                table: "Admins",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorServiceLocationId",
                table: "Appointments",
                column: "DoctorServiceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_FamilyMemberId",
                table: "Appointments",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_HospitalId",
                table: "Appointments",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseItems_DispenseRecordId",
                table: "DispenseItems",
                column: "DispenseRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseItems_MedicineId",
                table: "DispenseItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseRecords_PharmacistId",
                table: "DispenseRecords",
                column: "PharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseRecords_PrescriptionId",
                table: "DispenseRecords",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_HospitalId",
                table: "DoctorAvailabilities",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDocuments_DoctorId",
                table: "DoctorDocuments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_DoctorId",
                table: "DoctorProfiles",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_HospitalId",
                table: "Doctors",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecialityId",
                table: "Doctors",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorServiceLocations_DoctorId",
                table: "DoctorServiceLocations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorServiceLocations_HospitalId",
                table: "DoctorServiceLocations",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorServiceLocations_SpecialityId",
                table: "DoctorServiceLocations",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorServiceSlots_DoctorServiceLocationId",
                table: "DoctorServiceSlots",
                column: "DoctorServiceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorStaffs_DoctorId",
                table: "DoctorStaffs",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorStaffs_UserId",
                table: "DoctorStaffs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorVerifications_DoctorId",
                table: "DoctorVerifications",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugInteractions_MedicineIdA",
                table: "DrugInteractions",
                column: "MedicineIdA");

            migrationBuilder.CreateIndex(
                name: "IX_DrugInteractions_MedicineIdB",
                table: "DrugInteractions",
                column: "MedicineIdB");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalPharmacyDocuments_ExternalPharmacyId",
                table: "ExternalPharmacyDocuments",
                column: "ExternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_PatientId",
                table: "FamilyMembers",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalPharmacies_HospitalId",
                table: "InternalPharmacies",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalPharmacyInventories_InternalPharmacyId",
                table: "InternalPharmacyInventories",
                column: "InternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalPharmacyInventories_MedicineId",
                table: "InternalPharmacyInventories",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepAppointments_DoctorId",
                table: "MedicalRepAppointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepAppointments_MedicalRepId",
                table: "MedicalRepAppointments",
                column: "MedicalRepId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReps_UserId",
                table: "MedicalReps",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRepSlots_DoctorId",
                table: "MedicalRepSlots",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPreferredPharmacies_ExternalPharmacyId",
                table: "PatientPreferredPharmacies",
                column: "ExternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPreferredPharmacies_PatientId",
                table: "PatientPreferredPharmacies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_AppointmentId",
                table: "PaymentLogs",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyNotifications_PatientId",
                table: "PharmacyNotifications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrders_PatientId",
                table: "PharmacyOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrders_QuotationId",
                table: "PharmacyOrders",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrderStatusLogs_PharmacyOrderId",
                table: "PharmacyOrderStatusLogs",
                column: "PharmacyOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyQuotationItems_MedicineId",
                table: "PharmacyQuotationItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyQuotationItems_QuotationId",
                table: "PharmacyQuotationItems",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyQuotations_ExternalPharmacyId",
                table: "PharmacyQuotations",
                column: "ExternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyQuotations_PrescriptionRouteId",
                table: "PharmacyQuotations",
                column: "PrescriptionRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyRatings_ExternalPharmacyId",
                table: "PharmacyRatings",
                column: "ExternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyRatings_PatientId",
                table: "PharmacyRatings",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyRatings_PharmacyOrderId",
                table: "PharmacyRatings",
                column: "PharmacyOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MedicineId",
                table: "PrescriptionItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionQrCodes_PrescriptionId",
                table: "PrescriptionQrCodes",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionRoutes_ExternalPharmacyId",
                table: "PrescriptionRoutes",
                column: "ExternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionRoutes_InternalPharmacyId",
                table: "PrescriptionRoutes",
                column: "InternalPharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionRoutes_PrescriptionId",
                table: "PrescriptionRoutes",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_QrScanLogs_PrescriptionQrCodeId",
                table: "QrScanLogs",
                column: "PrescriptionQrCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_QrScanLogs_ScannedByUserId",
                table: "QrScanLogs",
                column: "ScannedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "DispenseItems");

            migrationBuilder.DropTable(
                name: "DoctorAvailabilities");

            migrationBuilder.DropTable(
                name: "DoctorDocuments");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");

            migrationBuilder.DropTable(
                name: "DoctorServiceSlots");

            migrationBuilder.DropTable(
                name: "DoctorStaffs");

            migrationBuilder.DropTable(
                name: "DoctorVerifications");

            migrationBuilder.DropTable(
                name: "DrugInteractions");

            migrationBuilder.DropTable(
                name: "ExternalPharmacyDocuments");

            migrationBuilder.DropTable(
                name: "InternalPharmacyInventories");

            migrationBuilder.DropTable(
                name: "InternalPharmacyStaffRequests");

            migrationBuilder.DropTable(
                name: "MedicalRepAppointments");

            migrationBuilder.DropTable(
                name: "MedicalRepSlots");

            migrationBuilder.DropTable(
                name: "NmcDoctorRecords");

            migrationBuilder.DropTable(
                name: "OtpStores");

            migrationBuilder.DropTable(
                name: "PatientPreferredPharmacies");

            migrationBuilder.DropTable(
                name: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "PharmacyNotifications");

            migrationBuilder.DropTable(
                name: "PharmacyOrderStatusLogs");

            migrationBuilder.DropTable(
                name: "PharmacyQuotationItems");

            migrationBuilder.DropTable(
                name: "PharmacyRatings");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "QrScanLogs");

            migrationBuilder.DropTable(
                name: "DispenseRecords");

            migrationBuilder.DropTable(
                name: "MedicalReps");

            migrationBuilder.DropTable(
                name: "PharmacyOrders");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "PrescriptionQrCodes");

            migrationBuilder.DropTable(
                name: "PharmacyQuotations");

            migrationBuilder.DropTable(
                name: "PrescriptionRoutes");

            migrationBuilder.DropTable(
                name: "ExternalPharmacies");

            migrationBuilder.DropTable(
                name: "InternalPharmacies");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "DoctorServiceLocations");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Hospitals");

            migrationBuilder.DropTable(
                name: "Specialities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
