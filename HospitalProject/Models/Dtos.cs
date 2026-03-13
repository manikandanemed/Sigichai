namespace HospitalProject.Models
{
    // =========================
    // REGISTRATION DTOs
    // =========================

    public record PatientRegDto(
        string Name,
        string Mobile,
        string Password,
        double? Lat,
        double? Lon
    );

    public record HospitalDoctorRegDto(
        int HospitalId,           // 🔥 REQUIRED
        string Name,
        string Mobile,
        string Password,
        string Specialization,
        int? SpecialityId,
        double? Lat,
        double? Lon
    );

    public record IndependentDoctorRegDto(
    string Name,
    string Mobile,
    string Password
    //string Specialization,
    //int? SpecialityId
    //double? Lat,
    //double? Lon
);


    public record AdminRegDto(
        string Name,
        string Mobile,
        string Password
    );

    // =========================
    // LOGIN / OTP DTOs
    // =========================

    public record LoginDto(
        string MobileNumber,
        string Password
    );

    public record VerifyOtpDto(
        string MobileNumber,
        string Otp
    );

    // =========================
    // APPOINTMENT DTOs
    // =========================

    public record BookAppDto(
        int DoctorId,
        int SlotId,
        int? FamilyMemberId
    );

    public record DoctorConsultDto(
      int AppId,
      string Diagnosis,
      string Prescription,
      decimal Fees,
      List<PrescriptionItemDto>? Medicines = null,
      int ValidityDays = 30,   // 👈 default 30 days
      int MaxRefills = 0       // 👈 default 0 = one-time use
  );

    // Date REMOVE
    public record SlotCreateDto(
        string State,
        string Area,
        int HospitalId,
        List<int> SpecialityIds,
        string TimeSlot
    );

    public record PatientTimeBookingDto(
     int HospitalId,
     int DoctorId,
     DateOnly Date,
     string TimeSlot,
     string ReasonForVisit   // 👈 ADD

 );



    public record FamilyTimeBookingDto(
    int HospitalId,
    int DoctorId,
    DateOnly Date,
    string TimeSlot,
    int FamilyMemberId,
    string ReasonForVisit   // 👈 ADD
);



    public record AddFamilyMemberDto(
    string Name,
    string Relationship
);



    public record AppointmentListDto(
    int AppointmentId,
    string PatientName,
    string? FamilyMemberName,
    string TimeSlot,
    string Status,
    int? QueueToken
);


    public record PaymentViewDto(
    int AppointmentId,
    string PatientName,
    string? FamilyMemberName,
    string DoctorName,
    string TimeSlot,
    decimal Fees,
    bool IsPaid,
    string Status
);


    public record ConfirmPaymentDto(
    int AppointmentId,
    string PaymentMode   // Cash / UPI / Card
);



//    public record HospitalCreateDto(
//    string Name,
//    string Address,
//    string Phone
//);


    public record HospitalSetupDto(
    string HospitalName,
    string Address,
    string Phone,
    string AdminName,
    string AdminMobile,
    string AdminPassword
);


    public record PatientProfileDto(
    string Name,
    string Mobile,
    double? Latitude,
    double? Longitude,
    List<FamilyMemberDto> FamilyMembers
);

    public record FamilyMemberDto(
        int Id,
        string Name,
        string Relationship
    );

    public record UpdatePatientProfileDto(
        string Name,
        double? Latitude,
        double? Longitude
    );


    public record PatientAppointmentDto(
    int AppointmentId,
    string TempToken,
    string DoctorName,
    string Specialization,
    string HospitalName,
    DateTime Date,
    string TimeSlot,
    string Status,
    string? FamilyMemberName   // ⭐ ADD
);

    public record HospitalListDto(
    int HospitalId,
    string Name,
    string Address,
    string Phone
);

    public record DoctorPublicDto(
        int DoctorId,
        string DoctorName,
        string Specialization,
        string HospitalName
    );

    public record SpecialityDto(
        string Name
    );

    public record SpecialityViewDto(
        int Id,
        string Name
    );

    public record SpecialityCreateDto(
        string Name
    );


    //    public record DoctorAppointmentDto(
    //    int AppointmentId,
    //    string PatientName,
    //    string? FamilyMemberName,
    //    DateTime Date,
    //    string TimeSlot,
    //    string Status,
    //    int? QueueToken,
    //    string? ReasonForVisit   // 👈 ADD
    //);


    public record DoctorAppointmentDto(
    int AppointmentId,
    string PatientName,
    string? FamilyMemberName,
    DateTime Date,
    string TimeSlot,
    string Status,
    int? QueueToken,

    // 🔹 Vitals
    string? BloodPressure,
    int? Pulse,
    decimal? Temperature,
    int? SpO2,

    // 🔹 Doctor data
    string? Diagnosis,
    string? Prescription,
    decimal? Fees,

    string? ReasonForVisit
);



    public record AdminAppointmentDto(
    int AppointmentId,
    string PatientName,
    string? FamilyMemberName,
    string DoctorName,
    string Specialization,
    DateTime Date,
    string TimeSlot,
    string Status,
    int? QueueToken,
    decimal Fees,
    bool IsPaid,
    string? ReasonForVisit,
    string? TempToken
);


    public record ForgotPasswordDto(
    string MobileNumber
);

    public record ResetPasswordDto(
        string MobileNumber,
        string Otp,
        string NewPassword
    );


    //********************************
    //Doctor profile Add  Method
    //********************************

    public record DoctorProfileCreateDto(
     string FatherOrHusbandName,
     DateOnly Dob,
     string RegistrationNumber,
     DateOnly DateOfRegistration,
     string StateCouncil,
     string Degree,
     int GraduationYear,
     string University,
     string PermanentAddress,
     string? UprnNumber
 );


    //********************************
    //Doctor profile View get Method
    //********************************
    public record DoctorProfileViewDto(
     int DoctorId,
     string DoctorName,
     string FatherOrHusbandName,
     DateOnly Dob,
     string RegistrationNumber,
     DateOnly DateOfRegistration,
     string StateCouncil,
     string Degree,
     int GraduationYear,
     string University,
     string PermanentAddress,
     string? UprnNumber
 );



    public record StaffCreateDto(
    string Name,
    string Mobile,
    string StaffRole   // Nurse / Reception
);

    public record StaffQueueDto(
        int AppointmentId,
        string PatientName,
        string? FamilyMemberName,
        string TimeSlot,
        int QueueToken,
        string? ReasonForVisit   // 👈
    );


    // =========================
    // DOCTOR VERIFICATION DTO
    // =========================
    public record DoctorVerificationDto(
        int DoctorId,
        string RegistrationNumber,
        int YearOfRegistration,
        string CouncilName
    );


    // =========================
    // ADMIN  VERIFICATION DOCTOR DTO
    // =========================

    public record AdminDoctorVerificationViewDto(
    int DoctorId,
    string DoctorName,
    string MobileNumber,
    string Specialization,
    string RegistrationNumber,
    int YearOfRegistration,
    string CouncilName,
    string VerificationStatus
);


    public record DoctorAdminCreateDto(
    string Name,
    string Mobile
);


    // =========================
    // HISTORY DTOs
    // =========================

    public record DoctorPatientHistoryDto(
        DateTime AppointmentDate,
        string? ReasonForVisit,
        string? Diagnosis,
        string? Prescription,
        decimal Fees
    );

    public record PatientHistoryDto(
        DateTime AppointmentDate,
        string DoctorName,
        string? ReasonForVisit,
        string? Diagnosis,
        string? Prescription,
        decimal Fees
    );



    // =========================
    // PATIENT PERSONAL DETAILS DTO
    // =========================
    //public record PatientPersonalDetailsDto(
    //    DateOnly Dob,
    //    string Gender,
    //    string BloodGroup,
    //    string Email
    //);


    public record PatientPersonalDetailsDto(
    DateOnly Dob,
    string Gender,
    string BloodGroup,
    string Email,

    string Address,
    string EmergencyContact,

    decimal HeightCm,
    decimal WeightKg,

    string MedicalHistory,
    string Allergies
);


    public record UpdatePatientPersonalDetailsDto(
    string Name,
    string Phone,
    DateOnly Dob,
    string Gender,
    string Address,
    string BloodGroup,
    string EmergencyContact,
    decimal? HeightCm,
    decimal? WeightKg,
    List<string> MedicalHistory,
    List<FamilyMemberInputDto> Family
);


    public record FamilyMemberInputDto(
    string Name,
    string Phone,
    DateOnly Dob,
    string Gender,
    string Relation,
    string Address,
    string BloodGroup,
    string EmergencyContact,
    decimal HeightCm,
    decimal WeightKg
);



    // =========================
    // PATIENT PERSONAL DETAILS DOCTOR VIEW
    // =========================
    public record PatientBasicDetailsDto(
    string Name,
    DateOnly? Dob,
    string? Gender,
    string? BloodGroup,
    string? Email
);





//Update vitals
    public record UpdateVitalsDto(
    int AppointmentId,
    string BloodPressure,
    int Pulse,
    decimal Temperature,
    int SpO2
);


// Doctor View Patient Vitals
    public record PatientWorkspaceDto(
    string PatientName,
    int Age,
    string Gender,

    string? Complaint,

    string? BloodPressure,
    int? Pulse,
    decimal? Temperature,
    int? SpO2
);


    // Public api for get user details
    public record PublicUserDetailsDto(
    int UserId,
    string Name,
    string Role,
    int? Age,
    string? Gender
);



    public record PatientPersonalDetailsViewDto(
    int Id,
    string Name,
    string Phone,
    DateOnly? Dob,
    string? Gender,
    string? Address,
    string? BloodGroup,
    string? EmergencyContact,
    decimal? HeightCm,
    decimal? WeightKg,
    List<string> MedicalHistory,
    List<FamilyMemberViewDto> Family
);

    public record FamilyMemberViewDto(
        int Id,
        string Name,
        string Phone,
        DateOnly? Dob,
        string Gender,
        string Relation,
        string Address,
    string BloodGroup,
    string EmergencyContact,
    decimal HeightCm,
    decimal WeightKg
    );


// patient can view queue line details
    public record PatientQueueStatusDto(
    int YourQueueToken,
    int? CurrentlyServingToken,
    int PatientsBeforeYou,
    int TotalCheckedIn
);


//***************************************
// Admin view doctor details for Add slot
//***************************************
    public record AdminDoctorViewDto(
        int DoctorId,
        string DoctorName,
        string MobileNumber,
        string Specialization,
        bool IsVerified,
        bool IsActive
    );

    public class MarkNoShowDto
    {
        public DateOnly Date { get; set; }
        public string TimeSlot { get; set; } = null!;
    }


    public record AdminListForDoctorDto(
    int AdminUserId,
    string Name,
    string MobileNumber,
    bool IsBlocked
    );


    //END SESION GET BY SLOT ID
    public record EndSessionBySlotDto(
    int SlotId
    );


    public record MedicalRepProfileDto(
    string Name, 
    string Mobile, 
    string Password,
    string CompanyName,
    string Designation,
    string Area, 
    string IdProofNumber
    );


    public record MedicalRepSlotCreateDto(
         int DoctorId,
         DateOnly Date,
        string TimeSlot,
        int MaxReps
    );



    public record MedicalRepTimeBookingDto
    (
    int DoctorId,
    DateOnly Date,
    string TimeSlot
);


    public class MedicalRepSlotResponseDto
    {
        public int SlotId { get; set; }
        public string TimeSlot { get; set; } = "";
    }


    public class MedicalRepBookingViewDto
    {
        public int AppointmentId { get; set; }
        public string MedicalRepName { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Status { get; set; } = "";
        public string TempToken { get; set; } = "";
        public int? QueueToken { get; set; }
    }


    public class MedicalRepConsultDto
    {
        public int AppointmentId { get; set; }
        public string DoctorNotes { get; set; } = "";
    }


    public class EndMedicalRepSessionBySlotDto
    {
        public int SlotId { get; set; }
    }



    public class DoctorListDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string HospitalName { get; set; } = "";
    }



    public class MedicalRepAppointmentViewDto
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = "";
        public string TempToken { get; set; } = "";
        public string? DoctorNotes { get; set; }   // ✅ ADD THIS

    }


    public class BookAppointmentDto
    {
        public int HospitalId { get; set; }    // 👈 add
        public int DoctorId { get; set; }
        public DateOnly Date { get; set; }
        public string TimeSlot { get; set; } = "";
        public string ReasonForVisit { get; set; } = "";
        public int? FamilyMemberId { get; set; }
    }

    public record InternalPharmacyCreateDto(
     string PharmacyName,
     string PhoneNumber,
     string Address
 );

    public record InternalPharmacyStaffRegisterDto(
        string Name,
        string MobileNumber,
        string Password
    );

    public record ApprovePharmacyStaffDto(
        int RequestId
    );

    public record MedicineCreateDto(
        string GenericName,
        string BrandName,
        string Category,
        string Unit,
        int ReorderLevel
    );

    public record MedicineViewDto(
        int Id,
        string GenericName,
        string BrandName,
        string Category,
        string Unit,
        int ReorderLevel
    );

    public record InventoryUpdateDto(
        int MedicineId,
        string BatchNumber,
        DateOnly ExpiryDate,
        int Quantity,
        string Barcode,
        decimal Price
    );

    public record InventoryViewDto(
        int Id,
        int MedicineId,
        string MedicineName,
        string BatchNumber,
        DateTime ExpiryDate,
        int Quantity,
        string Barcode,
        decimal Price
    );

    public record PrescriptionItemDto(
        int MedicineId,
        string Dosage,
        string Duration,
        string Instructions,
        int QuantityPrescribed,
        bool GenericSubstitutionAllowed = false
    );

    public record StructuredPrescriptionDto(
        int AppointmentId,
        string? DoctorNotes,
        List<PrescriptionItemDto> Items
    );

    public record DispenseItemRequestDto(
        int MedicineId,
        int QuantityDispensed,
        string? Barcode = null
    );

    public record DispenseRequestDto(
        int PrescriptionId,
        string? Remarks,
        List<DispenseItemRequestDto> Items
    );

    public record PharmacyQueueDto(
        int PrescriptionId,
        int AppointmentId,
        string PatientName,
        string DoctorName,
        DateTime CreatedAt,
        string Status
    );

    public record LowStockAlertDto(
        int MedicineId,
        string MedicineName,
        int CurrentStock,
        int ReorderLevel
    );

    public record MedicineStockDto(
        int MedicineId,
        string GenericName,
        string BrandName,
        string Category,
        string Unit,
        int CurrentStock
    );

    public record PatientDispenseHistoryDto(
        int DispenseId,
        DateTime DispenseDate,
        string PharmacistName,
        string Remarks,
        decimal TotalAmount,
        List<DispenseHistoryItemDto> Items
    );

    public record DispenseHistoryItemDto(
        string MedicineName,
        string BatchNumber,
        int QuantityDispensed,
        decimal PricePerUnit
    );

    public record PharmacyNotificationDto(
        int Id,
        string Message,
        DateTime CreatedAt,
        bool IsRead
    );

    public record DrugInteractionCreateDto(
        int MedicineIdA,
        int MedicineIdB,
        string Severity,
        string Description
    );

    public record DrugInteractionViewDto(
        int Id,
        string MedicineNameA,
        string MedicineNameB,
        string Severity,
        string Description
    );


    public class QrPayloadDto
    {
        public int PrescriptionId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        public int MaxRefills { get; set; }
        public List<QrMedicineDto> Medicines { get; set; } = new();
    }

    public class QrMedicineDto
    {
        public int MedicineId { get; set; }
        public string GenericName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int QuantityPrescribed { get; set; }
        public bool GenericSubstitutionAllowed { get; set; }
    }

    public record PrescriptionQrViewDto(
        int PrescriptionId,
        string PatientName,
        string DoctorName,
        DateTime GeneratedAt,
        DateTime ValidUntil,
        int MaxRefills,
        int UsedRefills,
        bool IsFullyUsed,
        string QrImageBase64
    );

    public record QrScanResultDto(
        bool IsValid,
        string? InvalidReason,
        int PrescriptionId,
        int PatientId,
        string PatientName,
        string DoctorName,
        DateTime ValidUntil,
        int RefillsRemaining,
        int RefillNumber,
        List<QrMedicineDto> Medicines
    );

    public record QrScanRequestDto(
        string QrPayload,
        string PharmacyName
    );

    public record QrScanLogViewDto(
        int LogId,
        int PrescriptionId,
        string PharmacyName,
        string ScannedByUserName,
        bool WasValid,
        string? InvalidReason,
        int RefillNumber,
        DateTime ScannedAt
    );

    //External pharmacy

    // =========================
    // EXTERNAL PHARMACY DTOs
    // =========================

    public record ExternalPharmacyRegisterDto(
        string PharmacyName,
        string OwnerName,
        string MobileNumber,
        string Password,
        string Address,
        string LicenseNumber,
        double Latitude,
        double Longitude,
        bool OffersHomeDelivery,
        decimal? DeliveryRadius
    );

    public record ExternalPharmacyApproveDto(
        int ExternalPharmacyId,
        bool IsApproved,
        string? RejectionReason
    );

    public record ExternalPharmacyViewDto(
        int Id,
        string PharmacyName,
        string OwnerName,
        string MobileNumber,
        string Address,
        string LicenseNumber,
        double Latitude,
        double Longitude,
        bool OffersHomeDelivery,
        decimal? DeliveryRadius,
        string Status,
        string? RejectionReason,
        DateTime RegisteredAt,
        bool IsActive,
        double AverageRating,
        int TotalRatings
    );

    public record ExternalPharmacyLoginDto(
        string MobileNumber,
        string Password
    );

    public record ExternalPharmacyListDto(
        int Id,
        string PharmacyName,
        string Address,
        bool OffersHomeDelivery,
        double AverageRating,
        string Status
    );

   //ProductAdmin 
    public record ProductAdminRegisterDto(
    string Name,
    string MobileNumber,
    string Password
    );

    // =========================
    // PRESCRIPTION ROUTING DTOs
    // =========================

    public record SetPreferredPharmacyDto(
        int ExternalPharmacyId
    );

    public record PrescriptionRouteViewDto(
        int RouteId,
        int PrescriptionId,
        string PharmacyType,
        string PharmacyName,
        string Status,
        DateTime RoutedAt,
        DateTime? RespondedAt
    );

    public record NearbyPharmacyDto(
        int ExternalPharmacyId,
        string PharmacyName,
        string Address,
        double Latitude,
        double Longitude,
        double DistanceKm,
        bool OffersHomeDelivery,
        double AverageRating
    );

    // =========================
    // QUOTATION DTOs
    // =========================

    public record QuotationItemDto(
        int MedicineId,
        bool IsAvailable,
        decimal PricePerUnit,
        int QuantityAvailable
    );

    public record SubmitQuotationDto(
        int PrescriptionRouteId,
        bool OffersDelivery,
        decimal? DeliveryCharge,
        string? Notes,
        List<QuotationItemDto> Items
    );

    public record QuotationItemViewDto(
        int MedicineId,
        string MedicineName,
        string BrandName,
        bool IsAvailable,
        decimal PricePerUnit,
        int QuantityAvailable
    );

    public record QuotationViewDto(
        int QuotationId,
        int PrescriptionRouteId,
        string PharmacyName,
        string PharmacyAddress,
        bool OffersDelivery,
        decimal? DeliveryCharge,
        decimal TotalAmount,
        string? Notes,
        string Status,
        DateTime QuotedAt,
        List<QuotationItemViewDto> Items
    );

    public record SelectQuotationDto(
        int QuotationId
    );


    // =========================
    // PHARMACY ORDER DTOs
    // =========================

    public record PlaceOrderDto(
        int QuotationId,
        string OrderType,        // Pickup | Delivery
        string? DeliveryAddress,
        string PaymentMode       // DirectToPharmacy | App
    );

    public record UpdateOrderStatusDto(
        int OrderId,
        string Status,           // Preparing | ReadyForPickup | OutForDelivery | Delivered | Collected
        string? Remarks
    );

    public record OrderStatusLogDto(
        string Status,
        string? Remarks,
        DateTime UpdatedAt
    );

    public record PharmacyOrderViewDto(
        int OrderId,
        int QuotationId,
        string PharmacyName,
        string OrderType,
        string? DeliveryAddress,
        string PaymentMode,
        string PaymentStatus,
        string Status,
        decimal TotalAmount,
        decimal? DeliveryCharge,
        DateTime CreatedAt,
        DateTime? DeliveredAt,
        List<OrderStatusLogDto> StatusLogs
    );

    // =========================
    // RATING DTOs
    // =========================

    public record SubmitRatingDto(
        int PharmacyOrderId,
        int Rating,        // 1 - 5
        string? Review
    );

    public record PharmacyRatingViewDto(
        int RatingId,
        string PatientName,
        int Rating,
        string? Review,
        DateTime RatedAt
    );

    public record PharmacyRatingSummaryDto(
        int ExternalPharmacyId,
        string PharmacyName,
        double AverageRating,
        int TotalRatings,
        List<PharmacyRatingViewDto> RecentReviews
    );


    // =========================
    // NMC AUTO FILL DTO
    // =========================

    public record NmcLookupDto(
        string RegistrationNumber
    );

    public record NmcDoctorRecordViewDto(
        string RegistrationNumber,
        string Name,
        string FatherOrHusbandName,
        DateOnly Dob,
        DateOnly DateOfRegistration,
        string StateCouncil,
        string Degree,
        int GraduationYear,
        string University,
        string PermanentAddress,
        string? UprnNumber
    );



    // =========================
    // DOCTOR SERVICE LOCATION DTOs
    // =========================

    public record ServiceSlotDto(
        string TimeSlot
    );

    public record DoctorServiceLocationDto(
        int HospitalId,
        int SpecialityId,
        List<string> TimeSlots
    );

    public record SaveServiceLocationsDto(
        List<DoctorServiceLocationDto> Locations
    );

    public record DoctorServiceSlotViewDto(
        int SlotId,
        string TimeSlot
    );

    public record DoctorServiceLocationViewDto(
        int LocationId,
        int HospitalId,
        string HospitalName,
        string State,
        string Area,
        int SpecialityId,
        string SpecialityName,
        bool IsActive,
        List<DoctorServiceSlotViewDto> Slots
    );

    public record HospitalByStateAreaDto(
        int HospitalId,
        string HospitalName,
        string State,
        string Area
    );


    // =========================
    // HOSPITAL DTOs
    // =========================

    public record HospitalCreateDto(
        string Name,
        string Address,
        string Phone,
        string State,
        string Area
    );

    public record BulkHospitalCreateDto(
        List<HospitalCreateDto> Hospitals
    );

    public record HospitalViewDto(
        int Id,
        string Name,
        string Address,
        string Phone,
        string State,
        string Area
    );


    public record DoctorProfileUpdateDto(
    string? Name,
    string? FatherOrHusbandName,
    DateOnly? Dob,
    string? RegistrationNumber,
    DateOnly? DateOfRegistration,
    string? StateCouncil,
    string? Degree,
    int? GraduationYear,
    string? University,
    string? PermanentAddress,
    string? UprnNumber
);


    // =========================
    // DOCTOR SLOT VIEW DTOs
    // =========================

    public record DoctorSlotSpecialityDto(
        int SpecialityId,
        string SpecialityName
    );

    public record DoctorSlotViewDto(
     int SlotId,
     int HospitalId,
     string HospitalName,
     string State,
     string Area,
     string TimeSlot,
     bool IsClosed,
     List<DoctorSlotSpecialityDto> Specialities
 );


}