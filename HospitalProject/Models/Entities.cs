using System;
using System.Collections.Generic;

namespace HospitalProject.Models
{
    // =========================
    // USER (MASTER TABLE)
    // =========================
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Patient / Doctor / Admin
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // 🔥 ADD THIS
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // 🔥 SOFT DELETE
        public bool IsDeleted { get; set; } = false;

        // Navigation (One-to-One)
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Admin? Admin { get; set; }

        public MedicalRep MedicalRep { get; set; }

    }

    // =========================
    // PATIENT
    // =========================
    public class Patient
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // 🔥 NEW PERSONAL DETAILS
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }       // Male / Female / Other
        public string? BloodGroup { get; set; }   // O+, A+, etc
        public string? Email { get; set; }


        // 🔹 NEW DETAILS
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        public decimal? HeightCm { get; set; }   // 170.5
        public decimal? WeightKg { get; set; }   // 68.2

        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }

        public ICollection<FamilyMember> FamilyMembers { get; set; }
            = new List<FamilyMember>();

    }

    // =========================
    // DOCTOR
    // =========================
    public class Doctor
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Specialization { get; set; } = string.Empty;

        // 🔥 Hospital optional
        public int? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }

        // 📍 Location
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // 🔐 Verification
        public bool IsVerified { get; set; } = false;

        public bool IsArrivedToday { get; set; } = false;
        public DateTime? ArrivedTime { get; set; }


        // 🔥 ADD THIS (VERY IMPORTANT)
        public ICollection<DoctorAvailability> Availabilities { get; set; }
            = new List<DoctorAvailability>();
    }




    // =========================
    // DOCTOR VERIFICATION
    // =========================

    public class DoctorVerification
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public string RegistrationNumber { get; set; } = "";
        public int YearOfRegistration { get; set; }
        public string CouncilName { get; set; } = "";

        public string VerificationStatus { get; set; } = "PENDING";
        public DateTime? VerifiedOn { get; set; }

        public string RawResponse { get; set; } = "";
    }


    // =========================
    // ADMIN
    // =========================
    public class Admin
    {
        public int Id { get; set; }

        // FK → User
        public int UserId { get; set; }
        public int HospitalId { get; set; }

        public User User { get; set; } = null!;

    }

    // =========================
    // DOCTOR AVAILABILITY (SLOTS)
    // =========================
    public class DoctorAvailability
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public DateTime AvailableDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // "10:00 - 11:00"

        public bool IsClosed { get; set; }   // ⭐ ADD THIS
    }

    // =========================
    // FAMILY MEMBER
    // =========================
    public class FamilyMember
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateOnly? Dob { get; set; }
        public string Gender { get; set; } = "";
        public string Relationship { get; set; } = "";

        // 🔥 HR REQUIRED FIELDS
        public string Address { get; set; } = "";
        public string BloodGroup { get; set; } = "";
        public string EmergencyContact { get; set; } = "";
        public decimal HeightCm { get; set; }
        public decimal WeightKg { get; set; }
    }


    // =========================
    // APPOINTMENT
    // =========================
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;

        public string TempToken { get; set; } = string.Empty;
        public int? QueueToken { get; set; }

        public string Status { get; set; } = "Booked";

        // Consultation
        public string? Diagnosis { get; set; }
        public string? Prescription { get; set; }

        // Payment
        public decimal Fees { get; set; }
        public bool IsPaid { get; set; }

        // 🔥 NEW
        public string? PaymentMode { get; set; }   // Cash / UPI / Card
        public DateTime? PaidAt { get; set; }

        public int? FamilyMemberId { get; set; }
        public FamilyMember? FamilyMember { get; set; }
        public int? HospitalId { get; set; }

        public string? ReasonForVisit { get; set; }   // 👈 ADD

        public string? BloodPressure { get; set; }
        public int? Pulse { get; set; }
        public decimal? Temperature { get; set; }
        public int? SpO2 { get; set; }

        // 🔥 One Appointment → Many PaymentLogs
        public ICollection<PaymentLog>? PaymentLogs { get; set; }

        //Razor payment
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
    }


    //Razor payment
    public class PaymentLog
    {
        public int Id { get; set; }
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }  // Navigation property

        public string RazorpayOrderId { get; set; } = string.Empty;
        public string? RazorpayPaymentId { get; set; }
        public decimal Amount { get; set; } // ₹10.00
        public string Status { get; set; } = "Created"; // Created, Captured, Failed, Refunded
        public string? RawResponse { get; set; } // Razorpay JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? FailureReason { get; set; }

    }



    // =========================
    // OTP STORE
    // =========================
    //public class OtpStore
    //{
    //    public int Id { get; set; }
    //    public string MobileNumber { get; set; } = string.Empty;
    //    public string OtpCode { get; set; } = string.Empty;
    //    public DateTime Expiry { get; set; }
    //}


    public class OtpStore
    {
        public int Id { get; set; }

        // 🔥 NEW
        public int? UserId { get; set; }      // nullable (old data safe)

        public string MobileNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;

        // 🔥 NEW
        public string? OtpType { get; set; }   // SMS / EMAIL
        public string? Purpose { get; set; }   // LOGIN / FORGOT / REGISTER

        // 🔥 NEW
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public DateTime Expiry { get; set; }

        // 🔥 NEW
        public bool IsUsed { get; set; } = false;
        public bool IsSent { get; set; } = false;
    }



    public class Hospital
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }



    public class DoctorProfile
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // Personal
        public DateOnly Dob { get; set; }
        public int Experience { get; set; }
        public string Languages { get; set; } = string.Empty;

        // Professional
        public string LicenseType { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string StateCouncil { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public int GraduationYear { get; set; }

        public string PracticeMode { get; set; } = string.Empty; // Clinic / Online
        public decimal ConsultationFee { get; set; }
    }


    public class DoctorDocument
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public string DocumentType { get; set; } = string.Empty;
        // License, Degree, IDProof

        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }


    public class DoctorStaff
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string StaffRole { get; set; } = string.Empty;
        // Nurse / Reception
    }



    public class MedicalRep
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string CompanyName { get; set; }
        public string Designation { get; set; }
        public string Area { get; set; }

        public string IdProofNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔥 SOFT DELETE
        public bool IsDeleted { get; set; } = false;
    }


    public class MedicalRepSlot
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime SlotDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;

        public int MaxReps { get; set; }      // 🔥 capacity
        public int BookedCount { get; set; }  // 🔥 how many booked

        public bool IsClosed { get; set; } = false;
    }


    public class MedicalRepAppointment
    {
        public int Id { get; set; }

        public int MedicalRepId { get; set; }
        public MedicalRep MedicalRep { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;

        public string TempToken { get; set; } = string.Empty;
        public int? QueueToken { get; set; }

        public string Status { get; set; } = "Booked";
        // Booked | CheckedIn | Consulted | Completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? DoctorNotes { get; set; }
    }





}
