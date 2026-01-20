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
        double? Lat,
        double? Lon
    );

    public record IndependentDoctorRegDto(
    string Name,
    string Mobile,
    string Password,
    string Specialization,
    double? Lat,
    double? Lon
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
        decimal Fees
    );

    public record SlotCreateDto(
     DateOnly AvailableDate,
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



    public record HospitalCreateDto(
    string Name,
    string Address,
    string Phone
);


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
    string Status
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
    string? ReasonForVisit   // 👈
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
    DateOnly Dob,
    int Experience,
    string Languages,
    string LicenseType,
    string LicenseNumber,
    string StateCouncil,
    string Degree,
    string University,
    int GraduationYear,
    string PracticeMode,
    decimal ConsultationFee
);


    //********************************
    //Doctor profile View get Method
    //********************************
    public record DoctorProfileViewDto(
    int DoctorId,
    string DoctorName,
    string Specialization,

    DateOnly Dob,
    int Experience,
    string Languages,

    string LicenseType,
    string LicenseNumber,
    string StateCouncil,

    string Degree,
    string University,
    int GraduationYear,

    string PracticeMode,
    decimal ConsultationFee
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






}