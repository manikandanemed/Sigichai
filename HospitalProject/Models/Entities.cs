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

        public int? SpecialityId { get; set; }
        public Speciality? Speciality { get; set; }
    }

    public class Speciality
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
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

        public int? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }


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
        public int? DoctorServiceLocationId { get; set; }
        public DoctorServiceLocation? DoctorServiceLocation { get; set; }

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

        // 👇 இதை add பண்ணுங்க
        public string State { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }



    //public class DoctorProfile
    //{
    //    public int Id { get; set; }

    //    public int DoctorId { get; set; }
    //    public Doctor Doctor { get; set; } = null!;

    //    // Personal
    //    public DateOnly Dob { get; set; }
    //    public int Experience { get; set; }
    //    public string Languages { get; set; } = string.Empty;

    //    // Professional
    //    public string LicenseType { get; set; } = string.Empty;
    //    public string LicenseNumber { get; set; } = string.Empty;
    //    public string StateCouncil { get; set; } = string.Empty;

    //    public string Degree { get; set; } = string.Empty;
    //    public string University { get; set; } = string.Empty;
    //    public int GraduationYear { get; set; }

    //    public string PracticeMode { get; set; } = string.Empty; // Clinic / Online
    //    public decimal ConsultationFee { get; set; }
    //}


    public class DoctorProfile
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public string FatherOrHusbandName { get; set; } = string.Empty;
        public DateOnly Dob { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateOnly DateOfRegistration { get; set; }
        public string StateCouncil { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string University { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
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


    public class InternalPharmacy
    {
        public int Id { get; set; }

        public int HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;

        public string PharmacyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }


    public class InternalPharmacyStaffRequest
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Hashed

        public string Status { get; set; } = "Pending";
        // Pending | Approved | Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    // =========================
    // MODULE 3: PHARMACY
    // =========================

    public class Medicine
    {
        public int Id { get; set; }
        public string GenericName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Tablet, Syrup, etc.
        public string Unit { get; set; } = string.Empty;     // mg, ml, etc.
        public int ReorderLevel { get; set; } = 10;
        public bool IsActive { get; set; } = true;
    }

    public class InternalPharmacyInventory
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int InternalPharmacyId { get; set; }
        public InternalPharmacy InternalPharmacy { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Prescription
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Dispensed, Partial, Cancelled
        public string? DoctorNotes { get; set; }
        public string? QRData { get; set; } // For future QR system

        // two lines add
        public int ValidityDays { get; set; } = 30;
        public int MaxRefills { get; set; } = 0;

        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }

    public class PrescriptionItem
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public string Dosage { get; set; } = string.Empty;     // e.g., "1-0-1"
        public string Duration { get; set; } = string.Empty;   // e.g., "5 days"
        public string Instructions { get; set; } = string.Empty; // e.g., "After food"
        public int QuantityPrescribed { get; set; }
        public int QuantityDispensed { get; set; }
        public bool GenericSubstitutionAllowed { get; set; } = false;
    }

    public class DispenseRecord
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public int PharmacistId { get; set; } // User.Id (Role: InternalPharmacyStaff)
        public User Pharmacist { get; set; } = null!;

        public DateTime DispensedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }

        public ICollection<DispenseItem> Items { get; set; } = new List<DispenseItem>();
    }

    public class DispenseItem
    {
        public int Id { get; set; }
        public int DispenseRecordId { get; set; }
        public DispenseRecord DispenseRecord { get; set; } = null!;

        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public string BatchNumber { get; set; } = string.Empty;
        public int QuantityDispensed { get; set; }
        public decimal PricePerUnit { get; set; }
    }

    public class PharmacyNotification
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }

    public class DrugInteraction
    {
        public int Id { get; set; }
        public int MedicineIdA { get; set; }
        public Medicine MedicineA { get; set; } = null!;
        public int MedicineIdB { get; set; }
        public Medicine MedicineB { get; set; } = null!;
        public string Severity { get; set; } = "High"; // High, Medium, Low
        public string Description { get; set; } = string.Empty;
    }

    public class PrescriptionQrCode
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;
        public string QrPayload { get; set; } = string.Empty;
        public string QrImageBase64 { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        public int MaxRefills { get; set; } = 0;
        public int UsedRefills { get; set; } = 0;
        public bool IsFullyUsed { get; set; } = false;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class QrScanLog
    {
        public int Id { get; set; }
        public int PrescriptionQrCodeId { get; set; }
        public PrescriptionQrCode PrescriptionQrCode { get; set; } = null!;
        public int ScannedByUserId { get; set; }
        public User ScannedByUser { get; set; } = null!;
        public string PharmacyName { get; set; } = string.Empty;
        public bool WasValid { get; set; }
        public string? InvalidReason { get; set; }
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
        public int RefillNumber { get; set; } = 1;
    }


    //External Pharmacy

    public class ExternalPharmacy
    {
        public int Id { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;

        // Location — nearby pharmacy find பண்ண
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Home Delivery
        public bool OffersHomeDelivery { get; set; } = false;
        public decimal? DeliveryRadius { get; set; } // KM

        // Status
        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
        public string? RejectionReason { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = false;

        // Rating
        public double AverageRating { get; set; } = 0.0;
        public int TotalRatings { get; set; } = 0;
    }

    public class ExternalPharmacyDocument
    {
        public int Id { get; set; }
        public int ExternalPharmacyId { get; set; }
        public ExternalPharmacy ExternalPharmacy { get; set; } = null!;
        public string DocumentType { get; set; } = string.Empty; // License, GST, Proof
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Patient-ஓட preferred external pharmacy save பண்ண
    /// </summary>
    public class PatientPreferredPharmacy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int ExternalPharmacyId { get; set; }
        public ExternalPharmacy ExternalPharmacy { get; set; } = null!;
        public DateTime SetAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Prescription எந்த pharmacy-க்கு route ஆச்சுன்னு track பண்ண
    /// </summary>
    public class PrescriptionRoute
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        // Internal or External
        public string PharmacyType { get; set; } = string.Empty; // "Internal" | "External"
        public int? InternalPharmacyId { get; set; }
        public InternalPharmacy? InternalPharmacy { get; set; }
        public int? ExternalPharmacyId { get; set; }
        public ExternalPharmacy? ExternalPharmacy { get; set; }

        public string Status { get; set; } = "Pending";
        // Pending | Accepted | Rejected | Filled | Cancelled

        public DateTime RoutedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }

    public class PharmacyQuotation
    {
        public int Id { get; set; }

        public int PrescriptionRouteId { get; set; }
        public PrescriptionRoute PrescriptionRoute { get; set; } = null!;

        public int ExternalPharmacyId { get; set; }
        public ExternalPharmacy ExternalPharmacy { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public bool OffersDelivery { get; set; } = false;
        public decimal? DeliveryCharge { get; set; }
        public string? Notes { get; set; }

        public string Status { get; set; } = "Pending";
        // Pending | Selected | Rejected

        public DateTime QuotedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PharmacyQuotationItem> Items { get; set; }
            = new List<PharmacyQuotationItem>();
    }

    public class PharmacyQuotationItem
    {
        public int Id { get; set; }

        public int QuotationId { get; set; }
        public PharmacyQuotation Quotation { get; set; } = null!;

        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public bool IsAvailable { get; set; } = true;
        public decimal PricePerUnit { get; set; }
        public int QuantityAvailable { get; set; }
    }

    public class PharmacyOrder
    {
        public int Id { get; set; }

        public int QuotationId { get; set; }
        public PharmacyQuotation Quotation { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        // Delivery or Pickup
        public string OrderType { get; set; } = "Pickup";
        // Pickup | Delivery

        public string? DeliveryAddress { get; set; }

        // Payment
        public string PaymentMode { get; set; } = "DirectToPharmacy";
        // DirectToPharmacy | App

        public string PaymentStatus { get; set; } = "Pending";
        // Pending | Paid

        // Order Status
        public string Status { get; set; } = "Confirmed";
        // Confirmed | Preparing | ReadyForPickup | OutForDelivery | Delivered | Collected

        public decimal TotalAmount { get; set; }
        public decimal? DeliveryCharge { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }

        public ICollection<PharmacyOrderStatusLog> StatusLogs { get; set; }
            = new List<PharmacyOrderStatusLog>();
    }

    public class PharmacyOrderStatusLog
    {
        public int Id { get; set; }

        public int PharmacyOrderId { get; set; }
        public PharmacyOrder PharmacyOrder { get; set; } = null!;

        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PharmacyRating
    {
        public int Id { get; set; }

        public int ExternalPharmacyId { get; set; }
        public ExternalPharmacy ExternalPharmacy { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int PharmacyOrderId { get; set; }
        public PharmacyOrder PharmacyOrder { get; set; } = null!;

        public int Rating { get; set; } // 1 - 5
        public string? Review { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.UtcNow;
    }


    public class NmcDoctorRecord
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FatherOrHusbandName { get; set; } = string.Empty;
        public DateOnly Dob { get; set; }
        public DateOnly DateOfRegistration { get; set; }
        public string StateCouncil { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string University { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
    }


    // =========================
    // DOCTOR SERVICE LOCATION
    // =========================
    public class DoctorServiceLocation
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;

        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DoctorServiceSlot> Slots { get; set; }
            = new List<DoctorServiceSlot>();
    }

    // =========================
    // DOCTOR SERVICE SLOT
    // =========================
    public class DoctorServiceSlot
    {
        public int Id { get; set; }

        public int DoctorServiceLocationId { get; set; }
        public DoctorServiceLocation DoctorServiceLocation { get; set; } = null!;

        public string TimeSlot { get; set; } = string.Empty;
        // "08:00-11:00" | "11:00-14:00" | "14:00-17:00"
    }
}
